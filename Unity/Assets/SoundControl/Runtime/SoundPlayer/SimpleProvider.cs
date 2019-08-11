using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{
	/// <summary>
	/// 最も単機能なプロバイダーです。
	/// </summary>
	public class SimpleProvider : IProvider<string>
	{
		AudioMixerGroup m_Group;
		Func<string, string> m_PathConversion;

		public SimpleProvider(AudioMixerGroup group)
		{
			m_Group = group;
			m_PathConversion = (x) => x;
		}
		public SimpleProvider(AudioMixerGroup group, Func<string, string> pathCreate)
		{
			m_Group = group;
			m_PathConversion = pathCreate;
		}

		public SimpleProvider(AudioMixerGroup group, string prefix, string format = null)
		{
			m_Group = group;
			m_PathConversion = (x) => prefix + ((format == null) ? x : string.Format(format, x));
		}

		public bool Load(string prm, Action<SoundInfo, Exception> onComplete)
		{
			string path = m_PathConversion?.Invoke(prm);
			var op = Resources.LoadAsync(path);
			op.completed += _ =>
			{
				var data = op.asset as SoundData;
				if (data != null)
				{
					onComplete?.Invoke(data.CreateInfo((x) => m_Group), null);
					return;
				}

				var clip = op.asset as AudioClip;
				if (clip == null)
				{
					onComplete?.Invoke(null, new System.IO.FileNotFoundException("Not Found AudioClip.", path));
				}
				else
				{
					SoundInfo info = new SoundInfo();
					info.Clip = clip;
					info.ControlId = path;
					info.Group = m_Group;
					onComplete?.Invoke(info, null);
				}
			};
			return true;
		}

	}

}
