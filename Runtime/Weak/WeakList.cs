using System;
using System.Collections.Generic;

/* TODO: switch to
 *
 * WeakReference<T>
 *
 */

namespace Edger.Unity.Weak {
    /*
     * There are some special logic for WeakBlock, since don't want to force all items
     * to implement OnAdded() and OnRemoved().
     */
    public sealed class WeakList<T> where T : class {
        private readonly List<WeakReference> _Elements = new List<WeakReference>();

        private int _LockCount = 0;
        private bool _NeedGc = false;
        private List<KeyValuePair<bool, T>> _Ops = null;

        public int Count {
            get {
                return _Elements.Count;
            }
        }

        public bool Add(T element) {
            if (_LockCount > 0) {
                if (!Contains(element)) {
                    if (_Ops == null) {
                        _Ops = new List<KeyValuePair<bool, T>>();
                    }
                    _Ops.Add(new KeyValuePair<bool, T>(true, element));
                    return true;
                } else {
                    return false;
                }
            } else {
                return DoAddElement(element);
            }
        }

        public bool Remove(T element) {
            if (_LockCount > 0) {
                if (Contains(element)) {
                    if (_Ops == null) {
                        _Ops = new List<KeyValuePair<bool, T>>();
                    }
                    _Ops.Add(new KeyValuePair<bool, T>(false, element));
                    return true;
                } else {
                    return false;
                }
            } else {
                return DoRemoveElement(element);
            }
        }

        public void Clear() {
            _Elements.Clear();
        }

        public int IndexOf(T element) {
            for (int i = 0; i < _Elements.Count; i++) {
                if (_Elements[i].IsAlive && _Elements[i].Target == element) {
                    return i;
                }
            }
            return -1;
        }

        public bool Contains(T element) {
            return IndexOf(element) >= 0;
        }

        private bool DoAddElement(T element) {
            if (!Contains(element)) {
                _Elements.Add(new WeakReference(element));
                WeakBlock block = element as WeakBlock;
                if (block != null) {
                    block.OnAdded();
                }
                return true;
            }
            return false;
        }

        private bool DoRemoveElement(T element) {
            int index = IndexOf(element);
            if (index >= 0) {
                WeakBlock block = element as WeakBlock;
                if (block != null) {
                    block.OnRemoved();
                }
                _Elements.RemoveAt(index);
                return true;
            }
            return false;
        }

        public int CollectAllGarbage() {
            int count = 0;
            int startIndex = 0;
            while (true) {
                startIndex = CollectOneGarbage(startIndex);
                if (startIndex < 0) {
                    break;
                }
                count++;
            }
            return count;
        }

        private int CollectOneGarbage(int startIndex) {
            int garbageIndex = -1;

            for (int i = startIndex; i < _Elements.Count; i++) {
                WeakReference element = _Elements[i];
                if (!element.IsAlive) {
                    if (Log.LogDebug) {
                        Log.Debug("Garbage Item In WeakList Found: {0}, {1}", this, element.Target);
                    }
                    garbageIndex = i;
                    break;
                }
            }

            if (garbageIndex >= 0) {
                _Elements.RemoveAt(garbageIndex);
                return garbageIndex;
            }
            return -1;
        }

        private T GetTarget(WeakReference element) {
            if (element.IsAlive) {
                return (T)element.Target;
            }
            return null;
        }

        private List<WeakReference> RetainLock() {
            _LockCount++;
            var result = _Elements;
            return result;
        }

        private void ReleaseLock(bool needGc) {
            if (needGc) {
                _NeedGc = true;
            }
            _LockCount--;
            if (_LockCount == 0) {
                if (_NeedGc) {
                    CollectAllGarbage();
                    _NeedGc = false;
                }
                if (_Ops != null) {
                    foreach (var op in _Ops) {
                        if (op.Key == true) {
                            DoAddElement(op.Value);
                        } else {
                            DoRemoveElement(op.Value);
                        }
                    }
                    _Ops.Clear();
                }
            }
        }

        public void ForEach(Action<T> callback) {
            bool needGc = false;
            foreach (var r in RetainLock()) {
                T element = GetTarget(r);
                if (element == null) {
                    needGc = true;
                } else {
                    callback(element);
                }
            }
            ReleaseLock(needGc);
        }
    }
}
