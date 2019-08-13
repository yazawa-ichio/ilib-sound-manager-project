using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ILib.Audio
{
	[CreateAssetMenu(menuName = "Create MusicData")]
	public class MusicData : ScriptableObject
	{
		public AudioClip Clip;
		public float Volume = 1f;
		public float Pitch = 1f;
		public string Group;

		public MusicInfo CreateMusic(System.Func<string, AudioMixerGroup> getGroup = null)
		{
			var music = new MusicInfo();
			music.Group = getGroup?.Invoke(Group) ?? null;
			music.Clip = Clip;
			music.Pitch = Pitch;
			music.Volume = Mathf.Clamp01(Volume);
			return music;
		}

	}
}
