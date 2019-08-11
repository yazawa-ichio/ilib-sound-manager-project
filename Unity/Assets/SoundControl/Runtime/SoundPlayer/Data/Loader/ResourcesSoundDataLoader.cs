using System;
using UnityEngine;

namespace ILib.Audio
{
	public class ResourcesSoundDataLoader : ISoundDataLoader<string>
	{
		Func<string, string> m_PathConversion;

		public ResourcesSoundDataLoader() => m_PathConversion = (x) => x;

		public ResourcesSoundDataLoader(Func<string, string> pathCreate) => m_PathConversion = pathCreate;

		public ResourcesSoundDataLoader(string prefix, string format = null)
		{
			m_PathConversion = (x) => prefix + ((format == null) ? x : string.Format(format, x));
		}

		public bool Load(string prm, Action<SoundData, Exception> onComplete)
		{
			string path = m_PathConversion?.Invoke(prm);
			var op = Resources.LoadAsync<SoundData>(path);
			op.completed += _ =>
			{
				var data = op.asset as SoundData;
				if (data == null)
				{
					onComplete?.Invoke(null, new System.IO.FileNotFoundException("Not Found SoundData.", path));
				}
				else
				{
					onComplete?.Invoke(data, null);
				}
			};
			return true;
		}

	}

	public class ResourcesSoundDataLoader<T> : ISoundDataLoader<T>
	{
		Func<T, string> m_PathCreate;
		
		public ResourcesSoundDataLoader(Func<T, string> pathCreate) => m_PathCreate = pathCreate;

		public bool Load(T prm, Action<SoundData, Exception> onComplete)
		{
			string path = m_PathCreate?.Invoke(prm);
			var op = Resources.LoadAsync<SoundData>(path);
			op.completed += _ =>
			{
				var data = op.asset as SoundData;
				if (data == null)
				{
					onComplete?.Invoke(data, null);
				}
				else
				{
					onComplete?.Invoke(data, null);
				}
			};
			return true;
		}
	}

}
