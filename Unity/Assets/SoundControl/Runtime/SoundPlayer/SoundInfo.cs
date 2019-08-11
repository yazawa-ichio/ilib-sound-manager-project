using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{
	public class SoundInfo
	{
		public string ControlId;
		public AudioClip Clip;
		public AudioMixerGroup Group;
		public float Volume = 1f;
		public float Pitch = 1f;
		public IStartControl PlayControl;
	}
}
