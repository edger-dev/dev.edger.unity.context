using System;
using System.Collections;
using System.Collections.Generic;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    public class DictAspect<T> : Aspect, IDictionary<string, T> {
        private Dictionary<string, T> _Elements = new Dictionary<string, T>();

        public T this[string key] { get => ((IDictionary<string, T>)_Elements)[key]; set => ((IDictionary<string, T>)_Elements)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, T>)_Elements).Keys;

        public ICollection<T> Values => ((IDictionary<string, T>)_Elements).Values;

        public int Count => ((ICollection<KeyValuePair<string, T>>)_Elements).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, T>>)_Elements).IsReadOnly;

        public void Add(string key, T value) {
            ((IDictionary<string, T>)_Elements).Add(key, value);
        }

        public void Add(KeyValuePair<string, T> item) {
            ((ICollection<KeyValuePair<string, T>>)_Elements).Add(item);
        }

        public void Clear() {
            ((ICollection<KeyValuePair<string, T>>)_Elements).Clear();
        }

        public bool Contains(KeyValuePair<string, T> item) {
            return ((ICollection<KeyValuePair<string, T>>)_Elements).Contains(item);
        }

        public bool ContainsKey(string key) {
            return ((IDictionary<string, T>)_Elements).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<string, T>>)_Elements).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() {
            return ((IEnumerable<KeyValuePair<string, T>>)_Elements).GetEnumerator();
        }

        public bool Remove(string key) {
            return ((IDictionary<string, T>)_Elements).Remove(key);
        }

        public bool Remove(KeyValuePair<string, T> item) {
            return ((ICollection<KeyValuePair<string, T>>)_Elements).Remove(item);
        }

        public bool TryGetValue(string key, out T value) {
            return ((IDictionary<string, T>)_Elements).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_Elements).GetEnumerator();
        }
    }
}
