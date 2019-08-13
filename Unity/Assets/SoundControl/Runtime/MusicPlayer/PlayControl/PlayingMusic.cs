using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{

	internal class PlayingMusic : System.IDisposable
	{

		object m_CurrentEntry;
		Transform m_Root;
		
		Stack<MusicObject> m_Pool = new Stack<MusicObject>();
		List<MusicObject> m_Playing = new List<MusicObject>();

		public int MaxPoolCount { get; set; } = 2;

		public PlayingMusic(Transform root)
		{
			m_Root = root;
		}

		public void SetCurrent(object entry)
		{
			m_CurrentEntry = entry;
		}

		public void Play(object entry, MusicRequest info, MusicPlayConfig config)
		{
			if (entry != m_CurrentEntry) return;
			var obj = Borrow();
			m_Playing.Add(obj);
			obj.PlayRequest(info, entry, config.FadeInTime);
		}

		public void Stop(float time)
		{
			foreach (var obj in m_Playing)
			{
				if (obj.Entry != m_CurrentEntry)
				{
					obj.Stop(time);
				}
			}
		}

		public void Pause(float time)
		{
			foreach (var obj in m_Playing)
			{
				if (obj.Entry == m_CurrentEntry)
				{
					obj.Pause(time);
				}
			}
		}

		public void Resume(float time)
		{
			foreach (var obj in m_Playing)
			{
				if (obj.Entry == m_CurrentEntry)
				{
					obj.Resume(time);
				}
			}
		}

		public void Update()
		{
			bool stop = false;
			for (int i = 0; i < m_Playing.Count; i++)
			{
				var obj = m_Playing[i];
				if (obj == null)
				{
					stop = true;
				}
				else if (!obj.Update())
				{
					Return(m_Playing[i]);
					m_Playing[i] = null;
					stop = true;
				}
			}
			if (stop)
			{
				m_Playing.RemoveAll((x) => x == null);
			}
		}

		MusicObject Borrow()
		{
			if (m_Pool.Count > 0)
			{
				return m_Pool.Pop();
			}
			else
			{
				return MusicObject.Create(m_Root);
			}
		}

		void Return(MusicObject obj)
		{
			//再生中のメイン分は引いておく
			if (m_Pool.Count < MaxPoolCount)
			{
				m_Pool.Push(obj);
			}
			else
			{
				obj.Destroy();
			}
		}

		public MusicInfo GetCacheInfo(object prm)
		{
			for (int i = 0; i < m_Playing.Count; i++)
			{
				var obj = m_Playing[i];
				if (obj != null && obj.Music != null && obj.Request != null)
				{
					var req = obj.Request;
					if (req.Param == prm)
					{
						return obj.Music;
					}
				}
			}
			return null;
		}

		public void Dispose()
		{
			GameObject.Destroy(m_Root.gameObject);
			m_Playing.Clear();
			m_Root = null;
		}

	}
}
