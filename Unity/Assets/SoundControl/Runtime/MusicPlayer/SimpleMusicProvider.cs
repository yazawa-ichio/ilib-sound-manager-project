using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{

	public class SimpleMusicProvider : IMusicProvider<string>
	{
		AudioMixerGroup m_Group;
		Func<string, string> m_PathConversion;

		public SimpleMusicProvider(AudioMixerGroup group)
		{
			m_Group = group;
			m_PathConversion = (x) => x;
		}
		public SimpleMusicProvider(AudioMixerGroup group, Func<string, string> pathCreate)
		{
			m_Group = group;
			m_PathConversion = pathCreate;
		}

		public SimpleMusicProvider(AudioMixerGroup group, string prefix, string format = null)
		{
			m_Group = group;
			m_PathConversion = (x) => prefix + ((format == null) ? x : string.Format(format, x));
		}

		public bool Load(string prm, Action<MusicInfo, Exception> onComplete)
		{
			string path = m_PathConversion?.Invoke(prm);
			var op = Resources.LoadAsync(path);
			op.completed += _ =>
			{
				var data = op.asset as MusicData;
				if (data != null)
				{
					onComplete?.Invoke(data.CreateMusic((x) => m_Group), null);
					return;
				}

				var clip = op.asset as AudioClip;
				if (clip == null)
				{
					onComplete?.Invoke(null, new System.IO.FileNotFoundException("Not Found AudioClip.", path));
				}
				else
				{
					MusicInfo music = new MusicInfo();
					music.Clip = clip;
					music.Volume = 1f;
					music.Pitch = 1f;
					onComplete?.Invoke(music, null);
				}
			};
			return true;
		}

	}

}
