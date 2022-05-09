using System;
using System.Collections;
using System.Collections.Generic;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    public abstract class ListAspect<T> : Aspect, IList<T> {
        private List<T> _Elements = new List<T>();

        public T this[int index] { get => ((IList<T>)_Elements)[index]; set => ((IList<T>)_Elements)[index] = value; }

        public int Count => ((ICollection<T>)_Elements).Count;

        public bool IsReadOnly => ((ICollection<T>)_Elements).IsReadOnly;

        public void Add(T item) {
            ((ICollection<T>)_Elements).Add(item);
        }

        public void Clear() {
            ((ICollection<T>)_Elements).Clear();
        }

        public bool Contains(T item) {
            return ((ICollection<T>)_Elements).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            ((ICollection<T>)_Elements).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerable<T>)_Elements).GetEnumerator();
        }

        public int IndexOf(T item) {
            return ((IList<T>)_Elements).IndexOf(item);
        }

        public void Insert(int index, T item) {
            ((IList<T>)_Elements).Insert(index, item);
        }

        public bool Remove(T item) {
            return ((ICollection<T>)_Elements).Remove(item);
        }

        public void RemoveAt(int index) {
            ((IList<T>)_Elements).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_Elements).GetEnumerator();
        }
    }
}
