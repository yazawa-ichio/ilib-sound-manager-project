using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{
	public interface IProvider<T>
	{
		bool Load(T prm, System.Action<SoundInfo, System.Exception> onComplete);
	}

	public static class IProviderExtension
	{
		public static SoundPlayer CreatePlayer(this IProvider<string> self, Transform root, int maxCount)
		{
			return new SoundPlayer(root, self, maxCount);
		}

		public static SoundPlayer<T> CreatePlayer<T>(this IProvider<T> self, Transform root, int maxCount) where T : ICacheKey
		{
			return new SoundPlayer<T>(root, self, maxCount);
		}
	}
}
