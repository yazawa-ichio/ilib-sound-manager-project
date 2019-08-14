using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{

	/// <summary>
	/// 参照が切れた際に自動で回収されるように
	/// MusicPlayerの実体を持つアクセサー
	/// </summary>
	internal class MusicPlayer : MusicPlayer<string>, IMusicPlayer
	{
		public MusicPlayer(MusicPlayerImpl<string> player) : base(player) { }
	}

	/// <summary>
	/// 参照が切れた際に自動で回収されるように
	/// MusicPlayerの実体を持つアクセサー
	/// </summary>
	internal class MusicPlayer<T> : IMusicPlayer<T>
	{
		MusicPlayerImpl<T> m_Player;

		public bool IsCacheInfoInStack { get => m_Player.IsCacheInfoInStack; set => m_Player.IsCacheInfoInStack = value; }

		public int MaxPoolCount { get => m_Player.MaxPoolCount; set => m_Player.MaxPoolCount = value; }

		public T Current => m_Player.Current;

		public MusicPlayer(MusicPlayerImpl<T> player)
		{
			m_Player = player;
		}

		~MusicPlayer()
		{
			Dispose();
		}

		public void Change(T prm, float time = 2, bool clearStack = false) => m_Player.Change(prm, time, clearStack);

		public void Change(T prm, MusicPlayConfig config, bool clearStack = false) => m_Player.Change(prm, config, clearStack);

		public void Pause(float time = 0.3F) => m_Player.Pause(time);

		public void Pop(float time = 2, bool startLastPosition = false) => m_Player.Pop(time, startLastPosition);

		public void Pop(MusicPlayConfig config, bool startLastPosition = false) => m_Player.Pop(config, startLastPosition);

		public void Push(T prm, float time = 2) => m_Player.Push(prm, time);

		public void Push(T prm, MusicPlayConfig config) => m_Player.Push(prm, config);

		public void Resume(float time = 0.3F) => m_Player.Resume(time);

		public void Stop(float time = 2) => m_Player.Stop(time);

		public void ClearStack() => m_Player.ClearStack();

		public void Dispose()
		{
			m_Player.Dispose();
			m_Player = null;
			System.GC.SuppressFinalize(this);
		}

	}

}
