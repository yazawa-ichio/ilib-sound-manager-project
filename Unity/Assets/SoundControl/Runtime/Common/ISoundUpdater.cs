namespace ILib.Audio
{
	/// <summary>
	/// サウンドデータの更新を行います。
	/// </summary>
	internal interface ISoundUpdater : System.IDisposable
	{
		void Update();
	}
}
