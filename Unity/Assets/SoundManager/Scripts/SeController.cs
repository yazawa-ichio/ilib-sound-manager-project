using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio.SoundManagement
{

	public class SeController
	{
		private ISoundPlayer m_Game;
		private ISoundPlayer m_UI;
		private ISoundPlayer m_Jingle;

		//プールを共有する設定なので一つだけの更新でいい
		public int MaxPoolCount
		{
			get => m_Game.MaxPoolCount;
			set => m_Game.MaxPoolCount = value;
		}

		public SeController(Config config)
		{
			Cache cache = new Cache();
			m_Game = config.GameSeProvider.Create(new SoundPlayerConfig { Cache = cache });
			//UI・Jingleはプールがなくても再生する
			m_Jingle = config.JingleProvider.Create(new SoundPlayerConfig { IsCreateIfNotEnough = true, Cache = cache });
			m_UI = config.UISEProvider.Create(new SoundPlayerConfig { IsCreateIfNotEnough = true, Cache = cache });
			//プールを共有する設定なので一つだけの更新でいい
			m_Game.MaxPoolCount = config.InitPoolCount;
		}

		/// <summary>
		/// SEを再生します。
		/// </summary>
		public IPlayingSoundContext Play(string prm)
		{
			return m_Game.Play(prm);
		}

		/// <summary>
		/// SEを再生します。
		/// </summary>
		public void PlayOneShot(string prm)
		{
			m_Game.PlayOneShot(prm);
		}

		/// <summary>
		/// UI用のSEを再生します。
		/// </summary>
		public void PlaySeFromUI(string prm)
		{
			m_UI.PlayOneShot(prm);
		}

		/// <summary>
		/// UI用のSEを再生します。
		/// </summary>
		public void PlaySeFromUI(string prm, out IPlayingSoundContext ctx)
		{
			ctx = m_UI.Play(prm);
		}

		/// <summary>
		/// シングルを再生します。
		/// </summary>
		public IPlayingSoundContext PlayJingle(string prm)
		{
			return m_Jingle.Play(prm);
		}

		/// <summary>
		/// キャッシュにサウンドを追加します。
		/// この関数でロードした場合、参照カウントを持たないためRemoveCache実行時に破棄されます。
		/// CreateCacheScopeで参照カウントがある場合は、参照カウントが0になるまで破棄されません。
		/// Voiceはキャッシュされません。
		/// </summary>
		public void AddCache(string prm, System.Action<bool, System.Exception> onLoad)
		{
			m_Game.AddCache(prm, onLoad);
		}

		/// <summary>
		/// キャッシュにサウンドを削除します。
		/// CreateCacheScopeで参照カウントがある場合は、参照カウントが0になるまで破棄されません。
		/// Voiceには影響しません
		/// </summary>
		public void RemoveCache(string prm)
		{
			m_Game.RemoveCache(prm);
		}

		/// <summary>
		/// 参照カウント方式でサウンド情報をキャッシュします。
		/// 解放する際は戻り値のDisposeを実行して下さい。
		/// Voiceはキャッシュされません。
		/// </summary>
		public ICacheScope CreateCacheScope(string[] keys)
		{
			return m_Game.CreateCacheScope(keys);
		}

		internal void Release()
		{
			m_Game?.Dispose();
			m_UI?.Dispose();
			m_Jingle?.Dispose();
		}

	}

}
