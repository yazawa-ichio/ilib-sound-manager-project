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

		int m_TotalCount;
		Stack<SoundObject> m_Stack = new Stack<SoundObject>();

		List<SoundObject> m_Playing = new List<SoundObject>();
		List<Request> m_Request = new List<Request>();

		public bool IsCreateIfNotEnough { get; set; }

		public int MaxPoolCount { get; set; } = 12;

		public PlayingList(Transform root)
		{
			m_Root = root;
		}

		public void ReservePool(int count = -1)
		{
			if (count < 0) count = MaxPoolCount;
			for (int i = m_TotalCount; i < count; i++)
			{
				m_Stack.Push(SoundObject.Create(this, m_Root));
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
			if (m_Stack.Count > 0)
			{
				return m_Stack.Pop();
			}
			else if (m_TotalCount < MaxPoolCount)
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
			if (m_Stack.Count < MaxPoolCount)
			{
				m_Stack.Push(obj);
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
			GameObject.Destroy(m_Root.gameObject);
			m_Playing.Clear();
			m_Stack.Clear();
			m_Request.Clear();
			m_Root = null;
		}
	}
}
