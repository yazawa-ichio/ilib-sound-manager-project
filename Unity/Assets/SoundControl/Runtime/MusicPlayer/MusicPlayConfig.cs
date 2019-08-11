namespace ILib.Audio
{
	public struct MusicPlayConfig
	{
		public float FadeInTime;
		public float FadeOutTime;
		public bool SkipLoadWait;
		public bool NoLoop;
		public bool IsOverrideEqualParam;

		public static MusicPlayConfig Get(float time)
		{
			MusicPlayConfig config = new MusicPlayConfig();
			config.FadeInTime = time;
			config.FadeOutTime = time;
			config.SkipLoadWait = false;
			return config;
		}

	}
}
