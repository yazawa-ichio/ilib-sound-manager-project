using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ILib.Audio
{

	public abstract class SoundPlayerBase<T> : IDisposable
	{

		public float Timeout { get; set; } = 2f;

		bool m_IsCreateIfNotEnough = false;
		public bool IsCreateIfNotEnough
		{
			get => m_IsCreateIfNotEnough;
			set
			{
				m_IsCreateIfNotEnough = value;
				if (m_PlayingList != null) m_PlayingList.IsCreateIfNotEnough = value;
			}
		}

		public bool IsAddCacheIfLoad { get; set; }

		protected IProvider<T> Provider { get; private set; }
		protected Cache<T> Cache { get; private set; }

		PlayingList m_PlayingList;
		bool m_Disposed;

		public SoundPlayerBase(Transform root, IProvider<T> provider, int maxPoolCount)
		{
			Provider = provider;
			Cache = new Cache<T>(provider);
			InitPlayingList(root, maxPoolCount);
		}

		protected void InitPlayingList(Transform root, int maxPoolCount)
		{
			m_PlayingList = new PlayingList(root, maxPoolCount);
			m_PlayingList.IsCreateIfNotEnough = m_IsCreateIfNotEnough;
		}

		protected internal abstract string GetCacheKey(T prm);

		public IPlayingSoundContext Play(T prm)
		{
			if (m_Disposed) return PlayingSoundContext.Empty;
			string key = GetCacheKey(prm);
			var ctx = new PlayingSoundContext();
			ctx.CreateTime = Time.unscaledTime;
			var info = Cache.GetInfo(key);
			if (info != null)
			{
				m_PlayingList.Play(info, ctx);
				return ctx;
			}
			else
			{
				ctx.IsLoading = true;
				ctx.LoadingTimeout = Timeout;
				var ret = Provider.Load(prm, (x, ex) =>
				{
					if (m_Disposed)
					{
						ctx.PlayFail();
						return;
					}
					OnLoad(x, ex, ctx);
					if (x != null && IsAddCacheIfLoad)
					{
						Cache.Add(key, false, x);
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
			var info = Cache.GetInfo(key);
			if (info != null)
			{
				m_PlayingList.Play(info, null);
			}
			else
			{
				var startTime = Time.unscaledTime;
				Provider.Load(prm, (x, ex) =>
				{
					if (m_Disposed) return;
					if (Time.unscaledDeltaTime - startTime < Timeout)
					{
						OnLoad(x, ex, null);
					}
					if (x != null && IsAddCacheIfLoad)
					{
						Cache.Add(key, false, x);
					}
				});
			}
		}

		public void PlayOneShot(SoundInfo info)
		{
			if (m_Disposed) return;
			m_PlayingList.Play(info, null);
		}

		public void AddCache(T prm, Action<bool,Exception> onLoad)
		{
			if (m_Disposed)
			{
				onLoad?.Invoke(false, new Exception("disposed sound player."));
				return;
			}
			Cache.Add(GetCacheKey(prm), prm, false, onLoad);
		}

		public void RemoveCache(T prm)
		{
			if (m_Disposed) return;
			Cache.Remove(GetCacheKey(prm), false);
		}

		public ICacheScope CreateCacheScope(T[] prms)
		{
			if (m_Disposed) return null;
			var keys = prms.Select(x => GetCacheKey(x)).ToArray();
			return Cache.CreateScope(keys, prms);
		}

		public void ClearCache(bool force = false)
		{
			if (m_Disposed) return;
			Cache.Clear(!force);
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
			if (m_Disposed) return;
			//キャッシュ更新
			Cache.Update();
			//再生リストを更新
			m_PlayingList.Update();
		}

		public void Dispose()
		{
			if (m_Disposed) return;
			m_Disposed = true;
			Cache.Clear(false);
			m_PlayingList.Dispose();
		}
	}

}
