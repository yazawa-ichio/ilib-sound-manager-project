using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{
	[CreateAssetMenu(menuName = "Create SoundData")]
	public class SoundData : ScriptableObject
	{
		public AudioClip Clip;
		public float Volume = 1f;
		public float Pitch = 1f;
		public string ControlId;
		public StartControl.Type ControlType;
		public float ControlParam1;

		public SoundInfo CreateInfo()
		{
			var info = new SoundInfo();
			info.ControlId = string.IsNullOrEmpty(ControlId) ? name : ControlId;
			info.Clip = Clip;
			info.Pitch = Pitch;
			info.Volume = Mathf.Clamp01(Volume);
			if (!string.IsNullOrEmpty(info.ControlId))
			{
				info.PlayControl = StartControl.Create(ControlType, ControlParam1);
			}
			return info;
		}

	}
}
