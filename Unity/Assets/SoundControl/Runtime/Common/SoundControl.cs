using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILib.Audio
{
	internal static class SoundControl
	{
		class SoundControlInstance : MonoBehaviour
		{
			private void Update()
			{
				SoundControl.Update();
			}
		}

		static bool s_Initialized = false;
		static SoundControlInstance s_Instance = null;
		static List<ISoundUpdater> s_Updater = new List<ISoundUpdater>();

		public static void Initialize()
		{
			if (s_Initialized) return;
			s_Initialized = true;
			var obj = new GameObject("SoundControl");
			GameObject.DontDestroyOnLoad(obj);
			s_Instance = obj.AddComponent<SoundControlInstance>();
		}

		public static void Release()
		{
			if (!s_Initialized) return;
			foreach (var updater in s_Updater.ToArray())
			{
				updater.Dispose();
			}
			s_Updater.Clear();
			GameObject.Destroy(s_Instance.gameObject, 0.1f);
			s_Instance = null;
			s_Initialized = false;
		}

		static Transform CreatePoolRoot(string name)
		{
			Initialize();
			GameObject obj = new GameObject(name);
			obj.transform.SetParent(s_Instance.transform);
			return obj.transform;
		}

		public static ISoundPlayer<T> CreateSoundPlayer<T>(ISoundProvider<T> provider, SoundPlayerConfig config = null)
		{
			var root = CreatePoolRoot(nameof(ISoundPlayer<T>) + ":" + provider);
			SoundPlayer<T> player = new SoundPlayer<T>(root, provider, config);
			s_Updater.Add(player);
			return new SoundPlayerRef<T>(player);
		}

		public static ISoundPlayer CreateSoundPlayer(ISoundProvider<string> provider, SoundPlayerConfig config = null)
		{
			var root = CreatePoolRoot(nameof(ISoundPlayer) + ":" + provider);
			SoundPlayer player = new SoundPlayer(root, provider, config);
			s_Updater.Add(player);
			return new SoundPlayerRef(player);
		}

		internal static void Remove(ISoundUpdater player, System.Action onRemove)
		{
			Initialize();
			s_Instance.StartCoroutine(_Remove(player, onRemove));
		}

		static IEnumerator _Remove(ISoundUpdater player, System.Action onRemove)
		{
			yield return null;
			s_Updater.Remove(player);
			onRemove?.Invoke();
		}

		static void Update()
		{
			for (int i = 0; i < s_Updater.Count; i++)
			{
				s_Updater[i].Update();
			}
		}


	}
}
