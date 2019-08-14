using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{
	/// <summary>
	/// 参照が切れた際に自動で回収されるように
	/// SoundPlayerの実体を持つアクセサー
	/// </summary>
	internal class SoundPlayer : SoundPlayer<string>, ISoundPlayer
	{
		public SoundPlayer(SoundPlayerImpl player) : base(player)
		{
		}
	}

	/// <summary>
	/// 参照が切れた際に自動で回収されるように
	/// SoundPlayerの実体を持つアクセサー
	/// </summary>
	internal class SoundPlayer<T> : ISoundPlayer<T>
	{
		SoundPlayerImpl<T> m_Player;
		public float LoadTimeout { get => m_Player.LoadTimeout; set => m_Player.LoadTimeout = value; }

		public bool IsCreateIfNotEnough { get => m_Player.IsCreateIfNotEnough; set => m_Player.IsCreateIfNotEnough = value; }

		public bool IsAddCacheIfLoad { get => m_Player.IsAddCacheIfLoad; set => m_Player.IsAddCacheIfLoad = value; }
		
		public int MaxPoolCount { get => m_Player.MaxPoolCount; set => m_Player.MaxPoolCount = value; }

		public SoundPlayer(SoundPlayerImpl<T> player)
		{
			m_Player = player;
		}

		~SoundPlayer()
		{
			Dispose();
		}

		public void ReservePool(int count = -1) => m_Player.ReservePool(count);

		public void AddCache(T prm, Action<bool, Exception> onLoad) => m_Player.AddCache(prm, onLoad);

		public void ClearCache(bool force = false) => m_Player.ClearCache(force);

		public ICacheScope CreateCacheScope(T[] prms) => m_Player.CreateCacheScope(prms);

		public IPlayingSoundContext Play(T prm) => m_Player.Play(prm);

		public IPlayingSoundContext Play(SoundInfo info) => m_Player.Play(info);

		public void PlayOneShot(T prm) => m_Player.PlayOneShot(prm);

		public void PlayOneShot(SoundInfo info) => m_Player.PlayOneShot(info);

		public void RemoveCache(T prm) => m_Player.RemoveCache(prm);

		public void Dispose()
		{
			m_Player.Dispose();
			m_Player = null;
			GC.SuppressFinalize(this);
		}

	}

}
