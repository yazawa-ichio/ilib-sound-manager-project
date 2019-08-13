using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{
	public class SoundPlayerConfig
	{
		/// <summary>
		/// キャッシュにない場合に遅延ロードをした際に
		/// 再生リクエストを破棄するタイムアウト時間です。
		/// </summary>
		public float LoadTimeout = 2f;
		/// <summary>
		/// 音源が不足していた際に、一時的に音源を追加作成するか？
		/// </summary>
		public bool IsCreateIfNotEnough;
		/// <summary>
		/// 音源プールの最大数です。
		/// この上限を超えた音源がプールに返却される場合破棄されます。
		/// </summary>
		public int MaxPoolCount = 12;
		/// <summary>
		/// キャッシュになく遅延ロードを行った際に
		/// ロードしたデータをキャッシュするか？
		/// </summary>
		public bool IsAddCacheIfLoad;
	}
}
