using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

namespace ILib.Audio.SoundManagement
{
#if !ILIB_AUDIO_DISABLE_TOOL_MENU
	[CreateAssetMenu(menuName = "ILib/Audio/SoundManagerConfig")]
#endif
	public class ConfigAsset : ConfigAssetBase
	{
	}

	public abstract class ConfigAssetBase : ScriptableObject
	{
		[Serializable]
		public class MusicConfig
		{
			[SerializeField]
			AudioMixerGroup m_MixerGroup = null;
			[SerializeField]
			string m_Format = "Audio/BGM/{0}";

			public MusicProvider CreateProvider(Func<string, Action<MusicInfo, Exception>, bool> load)
			{
				return new MusicProvider(m_MixerGroup, m_Format) { CustomLoad = load };
			}
		}

		[Serializable]
		public class SoundConfig
		{
			[SerializeField]
			AudioMixerGroup m_MixerGroup = null;
			[SerializeField]
			string m_Format = "Audio/SE/{0}";

			public SoundProvider CreateProvider(Func<string, Action<SoundInfo, Exception>, bool> load)
			{
				return new SoundProvider(m_MixerGroup, m_Format) { CustomLoad = load };
			}
		}



		[SerializeField]
		AudioMixer m_Mixer = null;

		[SerializeField, Header("ゲーム中に使用するBGM等の設定です")]
		MusicConfig m_Bgm = null;

		[SerializeField, Header("ゲーム中に使用する標準的なSEの設定です")]
		SoundConfig m_GameSE = null;

		[SerializeField, Header("UIに使用するサウンドエフェクトに影響しないSEの設定です")]
		SoundConfig m_UISE = null;

		[SerializeField, Header("ゲーム中に使用するシングル用の設定です")]
		SoundConfig m_JingleSE = null;

		[SerializeField, Header("ゲーム中に使用するボイス用の設定です")]
		SoundConfig m_VoiceSE = null;

		protected virtual Func<string, Action<MusicInfo, Exception>, bool> LoadMusic() => null;
		protected virtual Func<string, Action<SoundInfo, Exception>, bool> LoadSound() => null;
		protected virtual Func<string, Action<SoundInfo, Exception>, bool> LoadVoice() => null;

		public Config GetConfig()
		{
			var config = new Config();
			config.Mixer = m_Mixer;
			config.BgmProvider = m_Bgm.CreateProvider(LoadMusic());
			config.GameSeProvider = m_GameSE.CreateProvider(LoadSound());
			config.UISEProvider = m_UISE.CreateProvider(LoadSound());
			config.JingleProvider = m_JingleSE.CreateProvider(LoadSound());
			config.VoiceProvider = m_VoiceSE.CreateProvider(LoadSound());
			return config;
		}

	}

}
