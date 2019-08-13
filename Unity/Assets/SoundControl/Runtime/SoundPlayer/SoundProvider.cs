using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{
	/// <summary>
	/// パスでロードが解決できる場合のプロバイダーです。
	/// Resourcesをパスで探索します
	/// </summary>
	public class SoundProvider : SoundProviderBase<string>
	{
		public override string GetCacheKey(string prm)
		{
			return prm;
		}
	}

	/// <summary>
	/// パスでロードが解決できる場合のプロバイダーです。
	/// Resourcesをパスで探索します
	/// </summary>
	public class SoundProvider<T> : SoundProviderBase<T> where T : ICacheKey
	{
		public override string GetCacheKey(T prm)
		{
			return prm.GetCacheKey();
		}
	}

	/// <summary>
	/// パスでロードが解決できる場合のプロバイダーです。
	/// Resourcesをパスで探索します
	/// </summary>
	public class GeneralSoundProvider<T> : SoundProviderBase<T>
	{
		public override string GetCacheKey(T prm)
		{
			return prm.ToString();
		}
	}

	/// <summary>
	/// パスでロードが解決できる場合のプロバイダーです。
	/// Resourcesをパスで探索します
	/// </summary>
	public class IntKeySoundProvider<T> : SoundProviderBase<T> where T : struct, IConvertible
	{
		public override string GetCacheKey(T prm)
		{
			return prm.ToInt32(null).ToString();
		}
	}

	/// <summary>
	/// パスでロードが解決できる場合のプロバイダーです。
	/// Resourcesをパスで探索します
	/// </summary>
	public abstract class SoundProviderBase<T> : ISoundProvider<T>
	{

		Func<string, string> m_PathConversion;
		public Func<string, string> PathConversion { set => m_PathConversion = value; }

		Func<string, AudioMixerGroup> m_GroupSelector;
		public Func<string, AudioMixerGroup> GroupSelector { set => m_GroupSelector = value; }

		public SoundProviderBase<T> SetGroup(AudioMixerGroup group)
		{
			m_GroupSelector = (x) => group;
			return this;
		}

		public SoundProviderBase<T> SetPathFormat(string format)
		{
			m_PathConversion = x => string.Format(format, x);
			return this;
		}

		public abstract string GetCacheKey(T prm);

		string GetPath(T prm)
		{
			var key = GetCacheKey(prm);
			if (m_PathConversion != null)
			{
				key = m_PathConversion(key);
			}
			return key;
		}

		bool ISoundProvider<T>.Load(T prm, Action<SoundInfo, Exception> onComplete)
		{
			return Load(prm, onComplete);
		}

		protected virtual bool Load(T prm, Action<SoundInfo, Exception> onComplete)
		{
			string path = GetPath(prm);
			var op = Resources.LoadAsync(path);
			op.completed += _ =>
			{
				var data = op.asset as SoundData;
				if (data != null)
				{
					onComplete?.Invoke(data.CreateInfo(m_GroupSelector), null);
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
					if (m_GroupSelector != null)
					{
						info.Group = m_GroupSelector("");
					}
					onComplete?.Invoke(info, null);
				}
			};
			return true;
		}
	}

}
