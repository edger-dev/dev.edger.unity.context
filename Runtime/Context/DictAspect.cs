using System;
using System.Collections;
using System.Collections.Generic;

using Edger.Unity;
using Edger.Unity.Weak;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Edger.Unity.Context {
    public class DictAspect<TKey, TValue> : Aspect, IDictionary<TKey, TValue> {
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        private Dictionary<TKey, TValue> _Elements = new Dictionary<TKey, TValue>();

        public TValue this[TKey key] { get => ((IDictionary<TKey, TValue>)_Elements)[key]; set => ((IDictionary<TKey, TValue>)_Elements)[key] = value; }

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)_Elements).Keys;

        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)_Elements).Values;

        public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)_Elements).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_Elements).IsReadOnly;

        public void Add(TKey key, TValue value) {
            AdvanceRevision();
            ((IDictionary<TKey, TValue>)_Elements).Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            AdvanceRevision();
            ((ICollection<KeyValuePair<TKey, TValue>>)_Elements).Add(item);
        }

        public void Clear() {
            AdvanceRevision();
            ((ICollection<KeyValuePair<TKey, TValue>>)_Elements).Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_Elements).Contains(item);
        }

        public bool ContainsKey(TKey key) {
            return ((IDictionary<TKey, TValue>)_Elements).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            AdvanceRevision();
            ((ICollection<KeyValuePair<TKey, TValue>>)_Elements).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)_Elements).GetEnumerator();
        }

        public bool Remove(TKey key) {
            AdvanceRevision();
            return ((IDictionary<TKey, TValue>)_Elements).Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            AdvanceRevision();
            return ((ICollection<KeyValuePair<TKey, TValue>>)_Elements).Remove(item);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return ((IDictionary<TKey, TValue>)_Elements).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_Elements).GetEnumerator();
        }
    }
}
