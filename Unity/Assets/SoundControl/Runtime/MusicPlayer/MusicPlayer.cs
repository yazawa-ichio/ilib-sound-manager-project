using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{
	public class MusicPlayer : MusicPlayer<string>
	{
		public MusicPlayer(Transform root, IMusicProvider<string> provider) : base(root, provider) { }
	}

	public class MusicPlayer<T>
	{
		IMusicProvider<T> Provider;
		MusicStack m_MusicStack = new MusicStack();
		PlayingMusic m_PlayingMusic;

		public bool IsCacheIsStack
		{
			get => m_MusicStack.IsCacheInStack;
			set => m_MusicStack.IsCacheInStack = value;
		}

		public T Current
		{
			get
			{
				var cur = m_MusicStack.Current;
				if (cur != null)
				{
					return (T)cur;
				}
				return default;
			}
		}

		public MusicPlayer(Transform root, IMusicProvider<T> provider)
		{
			Provider = provider;
			m_PlayingMusic = new PlayingMusic(root, 2);
		}

		public void Change(T prm, float time = 2f)
		{
			Change(prm, MusicPlayConfig.Get(time));
		}

		public void Change(T prm, MusicPlayConfig config)
		{
			if (!config.IsOverrideEqualParam && (object)prm == m_MusicStack.Current)
			{
				return;
			}
			m_MusicStack.Pop();
			var push = m_MusicStack.Push(prm);
			Play(push, push.Main, config);
		}

		public void Push(T prm, float time = 2f)
		{
			Push(prm, MusicPlayConfig.Get(time));
		}

		public void Push(T prm, MusicPlayConfig config)
		{
			var cur = m_MusicStack.Current;
			var push = m_MusicStack.Push(prm);
			if (!config.IsOverrideEqualParam && cur == (object)prm)
			{
				return;
			}
			Play(push, push.Main, config);
		}

		public void Pop(float time = 2f, bool startLastPosition = false)
		{
			Pop(MusicPlayConfig.Get(time), startLastPosition);
		}

		public void Pop(MusicPlayConfig config, bool startLastPosition = false)
		{
			var cur = m_MusicStack.Current;
			var pop = m_MusicStack.Pop();
			if (pop == null)
			{
				Debug.Assert(false, "music stack is empty.");
				Stop(config.FadeOutTime);
				return;
			}
			if (!config.IsOverrideEqualParam && cur == pop.Main.Param)
			{
				return;
			}
			if (!startLastPosition)
			{
				pop.Main.Position = 0;
			}
			Play(pop, pop.Main, config);
		}

		public void Stop(float time = 2f)
		{
			m_PlayingMusic.SetCurrent(null);
			m_PlayingMusic.Stop(time);
		}

		public void Pause(float time = 0.3f)
		{
			m_PlayingMusic.Pause(time);
		}

		public void Resume(float time = 0.3f)
		{
			m_PlayingMusic.Resume(time);
		}

		void Play(MusicStack.Entry entry, MusicRequest req, MusicPlayConfig config)
		{
			m_PlayingMusic.SetCurrent(entry);

			if (req.Music != null)
			{
				m_PlayingMusic.Stop(config.FadeOutTime);
				m_PlayingMusic.Play(entry, req, config);
				return;
			}

			var cache = GetCacheInfo(req.Param);
			if (cache != null)
			{
				cache.AddRef();
				req.Music = cache;
				m_PlayingMusic.Stop(config.FadeOutTime);
				m_PlayingMusic.Play(entry, req, config);
				return;
			}

			if (config.SkipLoadWait)
			{
				m_PlayingMusic.Stop(config.FadeOutTime);
			}
			Provider.Load((T)req.Param, (info, ex) =>
			{
				if (!config.SkipLoadWait)
				{
					m_PlayingMusic.Stop(config.FadeOutTime);
				}
				if (ex != null)
				{

				}
				else
				{
					info.AddRef();
					req.Music = info;
					m_PlayingMusic.Play(entry, req, config);
				}
			});
		}

		MusicInfo GetCacheInfo(object prm)
		{
			return m_MusicStack.GetInfo(prm) ?? m_PlayingMusic.GetCacheInfo(prm) ?? null;
		}


		public void Update()
		{
			m_PlayingMusic.Update();
		}


	}
}
