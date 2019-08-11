using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{
	public interface IMusicProvider<T>
	{
		bool Load(T prm, System.Action<MusicInfo, System.Exception> onComplete);
	}
}
