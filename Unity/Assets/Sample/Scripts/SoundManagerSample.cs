using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using ILib.Debugs;
using ILib.Debugs.AutoRegisters;

namespace ILib.Audio.Sample
{
	[AutoRegisterTarget]
	public class SoundManagerSample : MonoBehaviour
	{
		ScDebugMenu m_Menu;

		private void Awake()
		{
			m_Menu = GetComponent<ScDebugMenu>();
			m_Menu.Contexts.Bind(this);
			m_Menu.Open();
		}

		void Update()
		{
			if (Input.GetMouseButtonDown(1))
			{
				m_Menu.Open();
			}
		}

		[DebugButton("PlaySe")]
		void PlaySe()
		{
			SoundManager.Se.Play("test1");
		}

		[DebugButton("PlaySePitch")]
		void PlaySePitch()
		{
			SoundManager.Se.Play("test1").Pitch = 2f;
		}


		[DebugButton("PlaySe FadeIn")]
		void PlaySeFadeIn()
		{
			SoundManager.Se.Play("test2").FadeIn(2f);
		}


		[DebugButton("PlaySe FadeOut")]
		void PlaySeFadeOut()
		{
			SoundManager.Se.Play("test2").FadeOut(2f);
		}

		[DebugButton("Push", Path = "BGM")]
		void BgmPush()
		{
			SoundManager.Bgm.Push("test" + (1 + ((int)Time.time) % 2));
		}

		[DebugButton("Pop", Path = "BGM")]
		void BgmPop()
		{
			SoundManager.Bgm.Pop();
		}


		[DebugButton("Pause", Path = "BGM")]
		void BgmPause()
		{
			SoundManager.Bgm.Pause();
		}

		[DebugButton("Resume",Path ="BGM")]
		void BgmResume()
		{
			SoundManager.Bgm.Resume();
		}

		[DebugButton("PlayVoice")]
		void PlayVoice()
		{
			SoundManager.Voice.Play("test1");
		}

	}

}
