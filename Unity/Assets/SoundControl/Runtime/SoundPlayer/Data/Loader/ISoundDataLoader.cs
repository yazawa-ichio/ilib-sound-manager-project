using System;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{
	public interface ISoundDataLoader<T>
	{
		bool Load(T prm, Action<SoundData, Exception> onComplete);
	}
}
