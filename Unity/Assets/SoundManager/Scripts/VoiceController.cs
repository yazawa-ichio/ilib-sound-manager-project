using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio.SoundManagement
{

	public class VoiceController
	{
		ISoundPlayer m_Player;

		public VoiceController(Config config)
		{
			m_Player = config.VoiceProvider.Create(new SoundPlayerConfig { IsCreateIfNotEnough = true });
		}

		public IPlayingSoundContext Play(string prm)
		{
			return m_Player.Play(prm);
		}

		public IPlayingSoundContext Play(SoundInfo info)
		{
			return m_Player.Play(info);
		}


		public void PlayOneShot(string prm)
		{
			m_Player.PlayOneShot(prm);
		}

		public void PlayOneShot(SoundInfo info)
		{
			m_Player.PlayOneShot(info);
		}

		internal void Release()
		{
			m_Player?.Dispose();
		}

	}

}
