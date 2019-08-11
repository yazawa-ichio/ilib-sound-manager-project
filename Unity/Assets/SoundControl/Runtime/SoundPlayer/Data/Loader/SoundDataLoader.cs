using System;
using System.Collections;
using System.Collections.Generic;

namespace ILib.Audio
{
	public class SoundDataLoader : SoundDataLoader<string>
	{
		public SoundDataLoader(Func<string, Action<SoundData, Exception>, bool> load) : base(load) { }

		public SoundDataLoader(Action<string, Action<SoundData, Exception>> load) : base(load) { }
	}

	public class SoundDataLoader<T> : ISoundDataLoader<T>
	{
		Func<T, Action<SoundData, Exception>,bool> m_Load;

		public SoundDataLoader(Func<T, Action<SoundData, Exception>, bool> load) => m_Load = load;

		public SoundDataLoader(Action<T, Action<SoundData, Exception>> load)
		{
			m_Load = (x, ex) =>
			{
				load(x, ex);
				return true;
			};
		}

		public bool Load(T prm, Action<SoundData, Exception> onComplete)
		{
			return m_Load(prm, onComplete);
		}
	}

}
