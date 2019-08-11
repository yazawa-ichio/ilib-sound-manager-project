using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ILib.Audio
{
	public class Cache<T>
	{
		class Entry
		{
			public SoundInfo Info;
			public bool Global;
			public int Count;
		}

		Dictionary<string, Entry> m_Cache = new Dictionary<string, Entry>();

		IProvider<T> m_Provider;
		Queue<Action> m_EventQueue = new Queue<Action>();

		public Cache(IProvider<T> provider)
		{
			m_Provider = provider;
		}

		public SoundInfo GetInfo(string key)
		{
			Entry entry;
			if (m_Cache.TryGetValue(key, out entry))
			{
				return entry.Info;
			}
			return null;
		}

		public void Add(string key, T prm, bool referenceCounting, Action<bool, Exception> onLoad)
		{
			Entry entry;
			if (!m_Cache.TryGetValue(key, out entry))
			{
				entry = m_Cache[key] = new Entry();
			}
			if (referenceCounting)
			{
				entry.Count++;
			}
			else
			{
				entry.Global = true;
			}
			if (entry.Info == null)
			{
				m_Provider.Load(prm, (x, ex) => OnLoad(key, onLoad, x, ex));
			}
			else
			{
				onLoad?.Invoke(true, null);
			}
		}

		public void Add(string key, bool referenceCounting, SoundInfo info)
		{
			Entry entry;
			if (!m_Cache.TryGetValue(key, out entry))
			{
				entry = m_Cache[key] = new Entry();
			}
			if (referenceCounting)
			{
				entry.Count++;
			}
			else
			{
				entry.Global = true;
			}
			entry.Info = info;
		}

		public void Remove(string key, bool referenceCounting)
		{
			Entry entry;
			if (m_Cache.TryGetValue(key, out entry))
			{
				if (referenceCounting)
				{
					entry.Count--;
				}
				else
				{
					entry.Global = false;
				}
				if (!entry.Global && entry.Count == 0)
				{
					m_Cache.Remove(key);
				}
			}
		}

		public void Clear(bool ignoreReferenceCounting = true)
		{
			if (!ignoreReferenceCounting)
			{
				m_Cache.Clear();
				return;
			}
			foreach (var key in m_Cache.Keys.ToArray())
			{
				var entry = m_Cache[key];
				entry.Global = false;
				if (entry.Count == 0)
				{
					m_Cache.Remove(key);
				}
			}
		}

		public ICacheScope CreateScope(string[] keys, T[] prms)
		{
			CacheScope scope = new CacheScope();
			scope.OnDispose = () =>
			{
				lock (m_EventQueue)
				{
					m_EventQueue.Enqueue(() =>
					{
						foreach (var key in keys)
						{
							Remove(key, true);
						}
					});
				}
			};
			int count = 0;
			bool success = true;
			for (int i = 0; i < keys.Length; i++)
			{
				Add(keys[i], prms[i], true, (ret, ex) =>
				{
					count++;
					if (ex != null) success = false;
					if (count == keys.Length)
					{
						scope.OnLoaded(success);
					}
				});
			}
			return scope;
		}

		void OnLoad(string key, Action<bool, Exception> onLoad, SoundInfo info, System.Exception ex)
		{
			if (ex == null)
			{
				onLoad?.Invoke(false, ex);
				return;
			}
			Entry entry;
			if (m_Cache.TryGetValue(key, out entry))
			{
				if (entry.Global || entry.Count > 0)
				{
					entry.Info = info;
					onLoad?.Invoke(true, null);
					return;
				}
			}
			onLoad?.Invoke(false, null);
		}


		internal void Update()
		{
			lock (m_EventQueue)
			{
				while (m_EventQueue.Count > 0)
				{
					m_EventQueue.Dequeue()?.Invoke();
				}
			}
		}


	}
}
