using System;
using System.Collections.Generic;

using UnityEngine;

using Edger.Unity;
using Edger.Unity.Weak;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Edger.Unity.Context {
    public interface IBusWatcher<TMsg> : IBlock {
        void OnBusMsg(Aspect bus, TMsg msg);
    }

    public sealed class BlockBusWatcher<TMsg> : WeakBlock, IBusWatcher<TMsg> {
        private readonly Action<Aspect, TMsg> _Block;

        public BlockBusWatcher(IBlockOwner owner, Action<Aspect, TMsg> block) : base(owner) {
            _Block = block;
        }

        public void OnBusMsg(Aspect bus, TMsg msg) {
            _Block(bus, msg);
        }
    }

    public interface IBusSub<TMsg> {
        void OnMsg(Aspect bus, TMsg msg);
    }

    public sealed class BlockBusSub<TMsg> : WeakBlock, IBusSub<TMsg> {
        private readonly Action<Aspect, TMsg> _Block;

        public BlockBusSub(IBlockOwner owner, Action<Aspect, TMsg> block) : base(owner) {
            _Block = block;
        }

        public void OnMsg(Aspect bus, TMsg msg) {
            _Block(bus, msg);
        }
    }

    public class MessageLog<TMsg> : AspectLog {
        public readonly TMsg Message;
        public MessageLog(Bus<TMsg> bus, TMsg msg) : base(bus) {
            Message = msg;
        }
    }

    public class Bus<TMsg> : Aspect {
        public Type MessageType { get => typeof(TMsg); }

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        public MessageLog<TMsg> Last { get; private set; }

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        private List<TMsg> _Msgs = null;

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        private WeakPubSub<TMsg, IBusSub<TMsg>> _MsgSubs = null;

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        private Dictionary<TMsg, object> _MsgTokens = null;

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        private Dictionary<TMsg, int> _MsgCounts = null;

        public bool WaitMsg(TMsg msg, Action<Aspect, TMsg, bool> callback) {
            if (GetMsgCount(msg) > 0) {
                callback(this, msg, false);
                return false;
            }
            bool called = false;
            BlockBusSub<TMsg> sub = new BlockBusSub<TMsg>(this, (Aspect _bus, TMsg _msg) => {
                if (!called) {
                    called = true;
                    callback(this, msg, true);
                }
            });
            AddSub(msg, sub);
            AddSub(msg, this, (Aspect _bus, TMsg _msg) => {
                _MsgSubs.RemoveSub(msg, sub);
            });
            return true;
        }

        private void TryAddMsg(TMsg msg) {
            if (_Msgs == null) {
                _Msgs = new List<TMsg>();
            }
            if (!_Msgs.Contains(msg)) {
                _Msgs.Add(msg);
            }
        }

        public void AddSub(TMsg msg, IBusSub<TMsg> sub) {
            TryAddMsg(msg);
            if (_MsgSubs == null) {
                _MsgSubs = new WeakPubSub<TMsg, IBusSub<TMsg>>();
            }
            _MsgSubs.AddSub(msg, sub);
            AdvanceRevision();
        }

        public BlockBusSub<TMsg> AddSub(TMsg msg, IBlockOwner owner, Action<Aspect, TMsg> block) {
            BlockBusSub<TMsg> result = new BlockBusSub<TMsg>(owner, block);
            AddSub(msg, result);
            return result;
        }

        public void RemoveSub(TMsg msg, IBusSub<TMsg> sub) {
            if (_MsgSubs != null) {
                _MsgSubs.RemoveSub(msg, sub);
                AdvanceRevision();
            }
        }

        private bool CheckToken(TMsg msg, object token) {
            if (_MsgTokens == null) {
                if (token == null) {
                    return true;
                }
                _MsgTokens = new Dictionary<TMsg, object>();
            }
            object oldToken;
            if (_MsgTokens.TryGetValue(msg, out oldToken)) {
                if (oldToken != token) {
                    Error("Invalid Token: {0}: {1} -> {2}", msg, oldToken, token);
                    return false;
                }
            } else if (token != null) {
                _MsgTokens[msg] = token;
            }
            return true;
        }

        public bool PublishOnce(TMsg msg, object token = null, bool isDebug = false) {
            if (IsMsgExist(msg)) {
                ErrorOrDebug(isDebug, "Already Published: {0}", msg);
                return false;
            }
            return Publish(msg, token);
        }

        public bool Publish(TMsg msg, object token = null) {
            TryAddMsg(msg);
            if (!CheckToken(msg, token)) {
                return false;
            }
            if (_MsgCounts == null) {
                _MsgCounts = new Dictionary<TMsg, int>();
            }
            _MsgCounts[msg] = GetMsgCount(msg) + 1;
            Last = new MessageLog<TMsg>(this, msg);
            AdvanceRevision();
            if (LogDebug) {
                Debug("Publish <{0}> {1}: sub_count = {2}, msg_count = {3}",
                    typeof(TMsg).FullName, msg, GetSubCount(msg), GetMsgCount(msg));
            }
            if (_MsgSubs != null) {
                _MsgSubs.Publish(msg, (IBusSub<TMsg> sub) => {
                    sub.OnMsg(this, msg);
                });
            }
            NotifyBusWatchers(msg);
            return true;
        }

        private void NotifyBusWatchers(TMsg msg) {
            WeakListUtil.ForEach(_BusWatchers, (watcher) => {
                watcher.OnBusMsg(this, msg);
            });
        }

        public bool Clear(TMsg msg, object token) {
            if (!CheckToken(msg, token)) {
                return false;
            }
            _MsgCounts[msg] = 0;
            AdvanceRevision();
            return true;
        }

        public int GetSubCount(TMsg msg) {
            if (_MsgSubs != null) {
                return _MsgSubs.GetSubCount(msg);
            }
            return 0;
        }

        public object GetMsgToken(TMsg msg) {
            if (_MsgTokens == null) return null;

            object token;
            if (_MsgTokens.TryGetValue(msg, out token)) {
                return token;
            }
            return null;
        }

        public int GetMsgCount(TMsg msg) {
            if (_MsgCounts == null) return 0;

            int count;
            if (_MsgCounts.TryGetValue(msg, out count)) {
                return count;
            }
            return 0;
        }

        public bool IsMsgExist(TMsg msg) {
            return GetMsgCount(msg) > 0;
        }

        public List<TMsg> GetExistMsgs() {
            List<TMsg> result = new List<TMsg>();
            if (_Msgs != null) {
                for (int i = 0; i < _Msgs.Count; i++) {
                    TMsg msg = _Msgs[i];
                    if (GetMsgCount(msg) > 0) {
                        result.Add(msg);
                    }
                }
            }
            return result;
        }

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        private WeakList<IBusWatcher<TMsg>> _BusWatchers = null;

        public int BusWatcherCount {
            get { return WeakListUtil.Count(_BusWatchers); }
        }

        public bool AddBusWatcher(IBusWatcher<TMsg> watcher) {
            return WeakListUtil.Add(ref _BusWatchers, watcher);
        }

        public bool RemoveBusWatcher(IBusWatcher<TMsg> watcher) {
            return WeakListUtil.Remove(_BusWatchers, watcher);
        }

        public void AddBusWatcher(IBlockOwner owner, Action<Aspect, TMsg> block) {
            AddBusWatcher(new BlockBusWatcher<TMsg>(owner, block));
        }
    }
}
