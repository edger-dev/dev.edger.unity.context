using System;
using System.Collections.Generic;

using UnityEngine;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    public interface IBusWatcher : IBlock {
        void OnBusMsg(Bus bus, string msg);
    }

    public sealed class BlockBusWatcher : WeakBlock, IBusWatcher {
        private readonly Action<Bus, string> _Block;

        public BlockBusWatcher(IBlockOwner owner, Action<Bus, string> block) : base(owner) {
            _Block = block;
        }

        public void OnBusMsg(Bus bus, string msg) {
            _Block(bus, msg);
        }
    }

    public interface IBusSub {
        void OnMsg(Bus bus, string msg);
    }

    public sealed class BlockBusSub : WeakBlock, IBusSub {
        private readonly Action<Bus, string> _Block;

        public BlockBusSub(IBlockOwner owner, Action<Bus, string> block) : base(owner) {
            _Block = block;
        }

        public void OnMsg(Bus bus, string msg) {
            _Block(bus, msg);
        }
    }

    [DisallowMultipleComponent()]
    public class Bus : Aspect {
        private List<string> _Msgs = null;
        private WeakPubSub<string, IBusSub> _MsgSubs = null;
        private Dictionary<string, object> _MsgTokens = null;
        private Dictionary<string, int> _MsgCounts = null;

        public bool WaitMsg(string msg, Action<Bus, string, bool> callback) {
            if (GetMsgCount(msg) > 0) {
                callback(this, msg, false);
                return false;
            }
            bool called = false;
            BlockBusSub sub = new BlockBusSub(this, (Bus _bus, string _msg) => {
                if (!called) {
                    called = true;
                    callback(this, msg, true);
                }
            });
            AddSub(msg, sub);
            AddSub(msg, this, (Bus _bus, string _msg) => {
                _MsgSubs.RemoveSub(msg, sub);
            });
            return true;
        }

        private void TryAddMsg(string msg) {
            if (_Msgs == null) {
                _Msgs = new List<string>();
            }
            if (!_Msgs.Contains(msg)) {
                _Msgs.Add(msg);
            }
        }

        public BlockBusWatcher AddBusWatcher(IBlockOwner owner, Action<Bus, string> block) {
            BlockBusWatcher result = new BlockBusWatcher(owner, block);
            if (AddBusWatcher(result)) {
                return result;
            }
            return null;
        }

        public void AddSub(string msg, IBusSub sub) {
            TryAddMsg(msg);
            if (_MsgSubs == null) {
                _MsgSubs = new WeakPubSub<string, IBusSub>();
            }
            _MsgSubs.AddSub(msg, sub);
        }

        public BlockBusSub AddSub(string msg, IBlockOwner owner, Action<Bus, string> block) {
            BlockBusSub result = new BlockBusSub(owner, block);
            AddSub(msg, result);
            return result;
        }

        public void RemoveSub(string msg, IBusSub sub) {
            if (_MsgSubs != null) {
                _MsgSubs.RemoveSub(msg, sub);
            }
        }

        private bool CheckToken(string msg, object token) {
            if (_MsgTokens == null) {
                _MsgTokens = new Dictionary<string, object>();
                _MsgCounts = new Dictionary<string, int>();
            }
            object oldToken;
            if (_MsgTokens.TryGetValue(msg, out oldToken)) {
                if (oldToken != token) {
                    Error("Invalid Token: {0}: {1} -> {2}", msg, oldToken, token);
                    return false;
                }
            } else {
                _MsgTokens[msg] = token;
            }
            return true;
        }

        public bool PublishOnce(string msg, object token, bool isDebug = false) {
            if (IsMsgExist(msg)) {
                ErrorOrDebug(isDebug, "Already Published: {0}", msg);
                return false;
            }
            return Publish(msg, token);
        }

        public bool Publish(string msg, object token) {
            TryAddMsg(msg);
            if (!CheckToken(msg, token)) {
                return false;
            }
            _MsgCounts[msg] = GetMsgCount(msg) + 1;

            if (_MsgSubs != null) {
                _MsgSubs.Publish(msg, (IBusSub sub) => {
                    sub.OnMsg(this, msg);
                });
            }

            NotifyBusWatchers(msg);

            if (LogDebug) {
                Debug("Publish {0}: sub_count = {1}, msg_count = {2}",
                        msg, GetSubCount(msg), GetMsgCount(msg));
            }
            return true;
        }

        private void NotifyBusWatchers(string msg) {
            WeakListUtil.ForEach(_BusWatchers, (watcher) => {
                watcher.OnBusMsg(this, msg);
            });
        }

        public bool Clear(string msg, object token) {
            if (!CheckToken(msg, token)) {
                return false;
            }
            _MsgCounts[msg] = 0;
            return true;
        }

        public int GetSubCount(string msg) {
            if (_MsgSubs != null) {
                return _MsgSubs.GetSubCount(msg);
            }
            return 0;
        }

        public object GetMsgToken(string msg) {
            if (_MsgTokens == null) return null;

            object token;
            if (_MsgTokens.TryGetValue(msg, out token)) {
                return token;
            }
            return null;
        }

        public int GetMsgCount(string msg) {
            if (_MsgCounts == null) return 0;

            int count;
            if (_MsgCounts.TryGetValue(msg, out count)) {
                return count;
            }
            return 0;
        }

        public bool IsMsgExist(string msg) {
            return GetMsgCount(msg) > 0;
        }

        public List<string> GetExistMsgs() {
            List<string> result = new List<string>();
            if (_Msgs != null) {
                for (int i = 0; i < _Msgs.Count; i++) {
                    string msg = _Msgs[i];
                    if (GetMsgCount(msg) > 0) {
                        result.Add(msg);
                    }
                }
            }
            return result;
        }

        private WeakList<IBusWatcher> _BusWatchers = null;

        public int BusWatcherCount {
            get { return WeakListUtil.Count(_BusWatchers); }
        }

        public bool AddBusWatcher(IBusWatcher listener) {
            return WeakListUtil.Add(ref _BusWatchers, listener);
        }

        public bool RemoveBusWatcher(IBusWatcher listener) {
            return WeakListUtil.Remove(_BusWatchers, listener);
        }
    }
}
