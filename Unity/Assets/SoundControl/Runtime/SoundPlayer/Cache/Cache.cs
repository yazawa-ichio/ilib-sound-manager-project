using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ILib.Audio
{
	internal class Cache
	{
		class Entry
		{
			public SoundInfo Info;
			public bool Global;
			public int Count;
		}

		int m_Revision = 0;
		Dictionary<string, Entry> m_Cache = new Dictionary<string, Entry>();
		Queue<Action> m_EventQueue = new Queue<Action>();

		public SoundInfo GetInfo(string key)
		{
			Entry entry;
			if (m_Cache.TryGetValue(key, out entry))
			{
				return entry.Info;
			}
			return null;
		}

		public void Add(string key, bool referenceCounting, ref bool cacheEmpty)
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
			cacheEmpty = entry.Info == null;
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
				lock (m_EventQueue)
				{
					m_Revision++;
				}
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

		public CacheScope CreateScope(string[] keys)
		{
			CacheScope scope = new CacheScope();
			scope.Revision = m_Revision;
			scope.OnDispose = () =>
			{
				lock (m_EventQueue)
				{
					if (scope.Revision != m_Revision)
					{
						//全キャッシュをした場合は古いスコープは無視する
						return;
					}
					m_EventQueue.Enqueue(() =>
					{
						foreach (var key in keys)
						{
							Remove(key, true);
						}
					});
				}
			};
			return scope;
		}

		public void OnLoad(string key, Action<bool, Exception> onLoad, SoundInfo info, Exception ex)
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
