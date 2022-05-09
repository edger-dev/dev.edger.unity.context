using System;
using System.Collections.Generic;

namespace Edger.Unity.Weak {
    public class WeakPubSub<TPub, TSub> where TSub : class {
        /*
         * Using List here since the list is mostly very short, also it's faster and more stable for Publish
         * Initialize in lazy way.
         */
        private Dictionary<int, WeakList<TSub>> _InstanceSubscribers = null;
        private WeakList<TSub> _ClassSubscribers = null;

        public int GetSubCount() {
            return WeakListUtil.Count(_ClassSubscribers);
        }

        public bool AddSub(TSub sub) {
            return WeakListUtil.Add<TSub>(ref _ClassSubscribers, sub);
        }

        public bool RemoveSub(TSub sub) {
            return WeakListUtil.Remove<TSub>(_ClassSubscribers, sub);
        }

        public int GetSubCount(TPub pub) {
            if (_InstanceSubscribers != null) {
                int pubHash = pub.GetHashCode();
                WeakList<TSub> subs = null;
                if (_InstanceSubscribers.TryGetValue(pubHash, out subs)) {
                    return subs.Count;
                }
            }
            return 0;
        }

        public bool AddSub(TPub pub, TSub sub) {
            if (_InstanceSubscribers == null) {
                _InstanceSubscribers = new Dictionary<int, WeakList<TSub>>();
            }
            int pubHash = pub.GetHashCode();
            WeakList<TSub> subs = null;
            if (!_InstanceSubscribers.TryGetValue(pubHash, out subs)) {
                subs = new WeakList<TSub>();
                _InstanceSubscribers[pubHash] = subs;
            }
            return subs.Add(sub);
        }

        public bool RemoveSub(TPub pub, TSub sub) {
            if (_InstanceSubscribers == null) {
                return false;
            }
            int pubHash = pub.GetHashCode();
            WeakList<TSub> subs = null;
            if (!_InstanceSubscribers.TryGetValue(pubHash, out subs)) {
                return false;
            }
            return subs.Remove(sub);
        }

        public void Publish(TPub pub, Action<TSub> callback) {
            if (_InstanceSubscribers != null) {
                int pubHash = pub.GetHashCode();
                WeakList<TSub> subs = null;
                if (_InstanceSubscribers.TryGetValue(pubHash, out subs)) {
                    subs.ForEach(callback);

                    if (subs.Count == 0) {
                        _InstanceSubscribers.Remove(pubHash);
                    }
                }
            }
            WeakListUtil.ForEach(_ClassSubscribers, callback);
        }

        public void RemovePub(TPub pub) {
            if (_InstanceSubscribers != null) {
                int pubHash = pub.GetHashCode();
                if (_InstanceSubscribers.ContainsKey(pubHash)) {
                    _InstanceSubscribers.Remove(pubHash);
                }
            }
        }
    }
}
