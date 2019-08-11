using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{

	internal class PlayingList : IPlayingList , IDisposable
	{
		struct Request
		{
			public SoundInfo Info;
			public PlayingSoundContext Context;
		}

		Transform m_Root;

		int m_PoolIndex;
		int m_TotalCount;
		SoundObject[] m_Stack;

		List<SoundObject> m_Playing = new List<SoundObject>();
		List<Request> m_Request = new List<Request>();

		public bool IsCreateIfNotEnough { get; set; }

		int m_MaxCount;

		public PlayingList(Transform root, int maxCount)
		{
			m_Root = root;
			m_MaxCount = maxCount;
			m_Stack = new SoundObject[m_MaxCount];
			 
			for (int i = 0; i < Mathf.Min(m_MaxCount, 16); i++)
			{
				m_Stack[m_PoolIndex++] = SoundObject.Create(this, m_Root);
				m_TotalCount++;
			}

		}

		public void Play(SoundInfo info, PlayingSoundContext context)
		{
			if (info.PlayControl == null)
			{
				PlayImpl(info, context);
			}
			else
			{
				switch (info.PlayControl.Update(this, info))
				{
					case StartControl.Result.Start:
						PlayImpl(info, context);
						break;
					case StartControl.Result.Wait:
						m_Request.Add(new Request { Info = info, Context = context });
						break;
				}
			}
		}

		void PlayImpl(SoundInfo info, PlayingSoundContext context)
		{
			var sound = Borrow();
			if (sound == null)
			{
				context.PlayFail();
				return;
			}
			sound.PlayRequest(info, context);
			m_Playing.Add(sound);
		}

		public void Update()
		{
			UpdateRequest();
			UpdateSoundObject();
		}

		void UpdateRequest()
		{
			bool remove = false;
			for (int i = 0; i < m_Request.Count; i++)
			{
				var info = m_Request[i].Info;
				switch (info.PlayControl.Update(this, info))
				{
					case StartControl.Result.Start:
						PlayImpl(info, m_Request[i].Context);
						m_Request[i] = default;
						remove = true;
						break;
					case StartControl.Result.Reject:
						m_Request[i] = default;
						remove = true;
						break;
				}
			}
			if (remove)
			{
				m_Request.RemoveAll((x) => x.Info == null);
			}
		}

		void UpdateSoundObject()
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

		public void Stop(SoundObject soundObject)
		{
			int index = m_Playing.IndexOf(soundObject);
			m_Playing[index] = null;
			Return(soundObject);
		}

		SoundObject Borrow()
		{
			if (m_PoolIndex > 0)
			{
				m_PoolIndex--;
				return m_Stack[m_PoolIndex];
			}
			else if (m_TotalCount < m_MaxCount)
			{
				m_TotalCount++;
				return SoundObject.Create(this, m_Root);
			}
			else if (IsCreateIfNotEnough)
			{
				return SoundObject.Create(this, m_Root);
			}
			return null;
		}

		void Return(SoundObject obj)
		{
			if (m_PoolIndex < m_MaxCount)
			{
				m_Stack[m_PoolIndex++] = obj;
			}
			else
			{
				obj.Destroy();
			}
		}

		int IPlayingList.GetCount(string controlId)
		{
			int count = 0;
			for (int i = 0; i < m_Playing.Count; i++)
			{
				var obj = m_Playing[i];
				if (obj != null && obj.ControlId == controlId)
				{
					count++;
				}
			}
			return count;
		}

		void IPlayingList.StopAll(string controlId)
		{
			for (int i = 0; i < m_Playing.Count; i++)
			{
				var obj = m_Playing[i];
				if (obj != null && obj.ControlId == controlId)
				{
					obj.Stop();
				}
			}
		}

		DateTime IPlayingList.GetLastPlayStartTime(string controlId)
		{
			DateTime last = default;
			for (int i = 0; i < m_Playing.Count; i++)
			{
				var obj = m_Playing[i];
				if (obj != null && obj.ControlId == controlId)
				{
					if (last < obj.PlayStartTime)
					{
						last = obj.PlayStartTime;
					}
				}
			}
			return last;
		}

		public void Dispose()
		{
			foreach (var obj in m_Playing.ToArray())
			{
				if(obj != null) obj.Destroy();
			}
			m_Playing.Clear();
			for (int i = 0; i < m_Stack.Length; i++)
			{
				if (m_Stack[i] != null)
				{
					m_Stack[i].Destroy();
					m_Stack[i] = null;
				}
			}
			m_Request.Clear();
			m_Root = null;
		}
	}
}
