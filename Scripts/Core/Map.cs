using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scuti {
    [Serializable]
    public class Map<T, K> : Dictionary<T, K>, IEnumerable<KeyValuePair<T, K>> {
        [SerializeField] List<T> m_Keys = new List<T>();
        [SerializeField] List<K> m_Values = new List<K>();

        new public K this[T t] {
            get {
                return Get(t);
            }
            set {
                Set(t, value);
            }
        }

        new public int Count {
            get { return m_Keys.Count; }
        }

        new public List<T> Keys {
            get { return m_Keys; }
        }

        new public List<K> Values {
            get { return m_Values; }
        }

        new public void Clear() {
            m_Keys.Clear();
            m_Values.Clear();
        }

        new public void Add(T t, K k) {
            if (m_Keys.Contains(t))
                throw new Exception("Map already contains the key");

            m_Keys.Add(t);
            m_Values.Add(k);
        }

        new public void Remove(T t) {
            if (!ContainsKey(t))
                throw new Exception("Map doesn't contain key " + t);

            int index = m_Keys.IndexOf(t);
            m_Keys.RemoveAt(index);
            m_Values.RemoveAt(index);
        }

        public K Get(T t) {
            int index = m_Keys.IndexOf(t);
            return m_Values[index];
        }

        public void Set(T t, K k) {
            if (!ContainsKey(t))
                throw new Exception("Map doesn't contain " + t);

            int keyIndex = m_Keys.IndexOf(t);
            m_Values[keyIndex] = k;
        }

        new public bool ContainsKey(T t) {
            return m_Keys.Contains(t);
        }

        new public bool ContainsValue(K k) {
            return m_Values.Contains(k);
        }

        new public IEnumerator<KeyValuePair<T, K>> GetEnumerator() {
            for (int i = 0; i < m_Keys.Count; i++) {
                var key = m_Keys[i];
                var value = m_Values[i];

                yield return new KeyValuePair<T, K>(key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
