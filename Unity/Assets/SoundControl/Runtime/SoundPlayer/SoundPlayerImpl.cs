using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ILib.Audio
{

	/// <summary>
	/// サウンドプレイヤーの実体です
	/// </summary>
	internal class SoundPlayerImpl : SoundPlayerImpl<string>, ISoundPlayer
	{
		public SoundPlayerImpl(Transform root, ISoundProvider<string> provider, SoundPlayerConfig config = null) : base(root, provider, config) { }
	}

	/// <summary>
	/// サウンドプレイヤーの実体です
	/// </summary>
	internal class SoundPlayerImpl<T> : ISoundPlayer<T> , ISoundUpdater
	{

		public float LoadTimeout { get; set; } = 2f;

		public bool IsCreateIfNotEnough
		{
			get => m_PlayingList.IsCreateIfNotEnough;
			set => m_PlayingList.IsCreateIfNotEnough = value;
		}

		public int MaxPoolCount
		{
			get => m_PlayingList.MaxPoolCount;
			set => m_PlayingList.MaxPoolCount = value;
		}

		public bool IsAddCacheIfLoad { get; set; }

		Cache m_Cache = new Cache();
		ISoundProvider<T> m_Provider;
		PlayingList m_PlayingList;
		bool m_Disposed;
		bool m_Removed;
		internal Action m_OnDisposed;

		public SoundPlayerImpl(Transform root, ISoundProvider<T> provider, SoundPlayerConfig config = null)
		{
			m_Provider = provider;
			m_PlayingList = new PlayingList(root);
			if (config != null)
			{
				LoadTimeout = config.LoadTimeout;
				MaxPoolCount = config.MaxPoolCount;
				IsCreateIfNotEnough = config.IsCreateIfNotEnough;
				IsAddCacheIfLoad = config.IsAddCacheIfLoad;
			}
		}

		public void ReservePool(int count = -1)
		{
			m_PlayingList.ReservePool(count);
		}

		protected string GetCacheKey(T prm)
		{
			return m_Provider.GetCacheKey(prm);
		}

		public IPlayingSoundContext Play(T prm)
		{
			if (m_Disposed) return PlayingSoundContext.Empty;
			string key = GetCacheKey(prm);
			var ctx = new PlayingSoundContext();
			ctx.CreateTime = Time.unscaledTime;
			var info = m_Cache.GetInfo(key);
			if (info != null)
			{
				m_PlayingList.Play(info, ctx);
				return ctx;
			}
			else
			{
				ctx.IsLoading = true;
				ctx.LoadingTimeout = LoadTimeout;
				var ret = m_Provider.Load(prm, (x, ex) =>
				{
					if (m_Disposed)
					{
						ctx.PlayFail();
						return;
					}
					OnLoad(x, ex, ctx);
					if (x != null && IsAddCacheIfLoad)
					{
						m_Cache.Add(key, false, x);
					}
				});
				return ret ? ctx : PlayingSoundContext.Empty;
			}
		}

		public IPlayingSoundContext Play(SoundInfo info)
		{
			if (m_Disposed) return PlayingSoundContext.Empty;
			var ctx = new PlayingSoundContext();
			ctx.CreateTime = Time.unscaledTime;
			m_PlayingList.Play(info, ctx);
			return ctx;
		}

		public void PlayOneShot(T prm)
		{
			if (m_Disposed) return;
			var key = GetCacheKey(prm);
			var info = m_Cache.GetInfo(key);
			if (info != null)
			{
				m_PlayingList.Play(info, null);
			}
			else
			{
				var startTime = Time.unscaledTime;
				m_Provider.Load(prm, (x, ex) =>
				{
					if (m_Disposed) return;
					if (Time.unscaledDeltaTime - startTime < LoadTimeout)
					{
						OnLoad(x, ex, null);
					}
					if (x != null && IsAddCacheIfLoad)
					{
						m_Cache.Add(key, false, x);
					}
				});
			}
		}

		public void PlayOneShot(SoundInfo info)
		{
			if (m_Disposed) return;
			m_PlayingList.Play(info, null);
		}

		public void AddCache(T prm, Action<bool, Exception> onLoad)
		{
			if (m_Disposed)
			{
				onLoad?.Invoke(false, new Exception("disposed sound player."));
				return;
			}
			var key = GetCacheKey(prm);
			var cacheEmpty = false;
			m_Cache.Add(key, false, ref cacheEmpty);
			if (cacheEmpty)
			{
				m_Provider.Load(prm, (x, ex) => m_Cache.OnLoad(key, onLoad, x, ex));
			}
			else
			{
				onLoad?.Invoke(true, null);
			}
		}

		public void RemoveCache(T prm)
		{
			if (m_Disposed) return;
			m_Cache.Remove(GetCacheKey(prm), false);
		}

		public ICacheScope CreateCacheScope(T[] prms)
		{
			if (m_Disposed) return null;
			var keys = prms.Select(x => GetCacheKey(x)).ToArray();
			var scope = m_Cache.CreateScope(keys);
			int count = 0;
			bool success = true;
			for (int i = 0; i < keys.Length; i++)
			{
				var cacheEmpty = false;
				m_Cache.Add(keys[i], false, ref cacheEmpty);
				if (!cacheEmpty)
				{
					count++;
					//完了チェック
					if (count == keys.Length) scope.OnLoaded(success);
				}
				else
				{
					m_Provider.Load(prms[i], (ret, ex) =>
					{
						count++;
						if (ex != null) success = false;
						//完了チェック
						if (count == keys.Length) scope.OnLoaded(success);
					});
				}
			}
			return scope;
		}

		public void ClearCache(bool force = false)
		{
			if (m_Disposed) return;
			m_Cache.Clear(!force);
		}

		internal void OnLoad(SoundInfo info, Exception error, PlayingSoundContext context)
		{
			if (m_Disposed || error != null || info == null)
			{
				context?.PlayFail();
				return;
			}
			//タイムアウト判定
			if (context != null && (Time.unscaledDeltaTime - context.CreateTime > context.LoadingTimeout))
			{
				context?.PlayFail();
				return;
			}
			if (context != null) context.IsLoading = false;
			m_PlayingList.Play(info, context);
		}

		public void Update()
		{
			if (m_Disposed)
			{
				Remove();
				return;
			}
			//キャッシュ更新
			m_Cache.Update();
			//再生リストを更新
			m_PlayingList.Update();
		}

		void Remove()
		{
			if (!m_Removed)
			{
				m_Removed = true;
				SoundControl.Remove(this, () =>
				{
					m_Cache.Clear(false);
					m_PlayingList.Dispose();
				});
			}
		}

		public void Dispose()
		{
			if (m_Disposed) return;
			m_Disposed = true;
		}

	}

}
