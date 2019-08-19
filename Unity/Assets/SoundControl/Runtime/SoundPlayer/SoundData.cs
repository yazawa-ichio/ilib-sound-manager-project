using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{
#if !ILIB_AUDIO_DISABLE_TOOL_MENU
	[CreateAssetMenu(menuName = "ILib/Audio/SoundData")]
#endif
	public class SoundData : ScriptableObject
	{
		[Header("音源")]
		public AudioClip Clip;
		[Header("音量")]
		public float Volume = 1f;
		[Header("ピッチ")]
		public float Pitch = 1f;
		[Header("再生管理用のID")]
		public string ControlId;
		[Header("再生管理の方式")]
		public StartControl.Type ControlType;
		public float ControlParam1;
		public float ControlParam2;

		public SoundInfo CreateInfo()
		{
			var info = new SoundInfo();
			info.ControlId = string.IsNullOrEmpty(ControlId) ? name : ControlId;
			info.Clip = Clip;
			info.Pitch = Pitch;
			info.Volume = Mathf.Clamp01(Volume);
			if (!string.IsNullOrEmpty(info.ControlId))
			{
				info.PlayControl = StartControl.Create(ControlType, ControlParam1, ControlParam2);
			}
			return info;
		}

	}
}
