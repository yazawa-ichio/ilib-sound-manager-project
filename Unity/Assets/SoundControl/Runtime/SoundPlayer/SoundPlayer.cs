using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{
	public interface ICacheKey
	{
		string GetCacheKey();
	}

	public class SoundPlayer : SoundPlayerBase<string>
	{
		public SoundPlayer(Transform root, IProvider<string> provider, int maxPoolCount) : base(root, provider, maxPoolCount) { }

		protected internal override string GetCacheKey(string prm) => prm;
	}

	public class SoundPlayer<T> : SoundPlayerBase<T> where T : ICacheKey
	{
		public SoundPlayer(Transform root, IProvider<T> provider, int maxPoolCount) : base(root, provider, maxPoolCount) { }

		protected internal override string GetCacheKey(T prm) => prm.GetCacheKey();
	}

}
