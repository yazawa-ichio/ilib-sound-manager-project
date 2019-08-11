using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{
	public class SoundDataProvider : SoundDataProvider<string>
	{
		public SoundDataProvider(Func<string, AudioMixerGroup> getGroup, ISoundDataLoader<string> loader) : base(getGroup, loader) { }

		public SoundDataProvider(AudioMixer audioMixer, ISoundDataLoader<string> loader) : base(audioMixer, loader) { }

		public SoundDataProvider(AudioMixerGroup group, ISoundDataLoader<string> loader) : base(group, loader) { }
	}

	public class SoundDataProvider<T> : IProvider<T>
	{
		ISoundDataLoader<T> m_Loader;
		Func<string, AudioMixerGroup> m_GetGroup;

		public SoundDataProvider(Func<string, AudioMixerGroup> getGroup, ISoundDataLoader<T> loader)
		{
			m_Loader = loader;
			m_GetGroup = getGroup;
		}

		public SoundDataProvider(AudioMixer audioMixer, ISoundDataLoader<T> loader)
		{
			m_Loader = loader;
			m_GetGroup = group => audioMixer.FindMatchingGroups(group)[0];
		}

		public SoundDataProvider(AudioMixerGroup group, ISoundDataLoader<T> loader)
		{
			m_Loader = loader;
			m_GetGroup = _ => group;
		}

		public bool Load(T prm, Action<SoundInfo, Exception> onComplete)
		{
			return m_Loader.Load(prm, (data, ex) =>
			{
				if (ex != null)
				{
					onComplete?.Invoke(null, ex);
				}
				else
				{
					onComplete?.Invoke(data.CreateInfo(m_GetGroup), null);
				}
			});
		}

	}

}
