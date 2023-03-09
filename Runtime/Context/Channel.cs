using System;
using System.Collections.Generic;

using UnityEngine;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    public class EventLog<TEvt> : AspectLog {
        public readonly TEvt Event;
        public EventLog(Channel<TEvt> channel, TEvt evt) : base(channel) {
            Event = evt;
        }
    }

    public abstract class Channel<TEvt> : Aspect {
        public Type EventType { get => typeof(TEvt); }

        public EventLog<TEvt> Last { get; private set; }

        public void FireEvent(TEvt evt) {
            Last = new EventLog<TEvt>(this, evt);
            AdvanceRevision();
            if (LogDebug) {
                Debug("FireEvent: {0}", evt.ToString());
            }
            NotifyChannelWatchers(Last);
        }

        private void NotifyChannelWatchers(EventLog<TEvt> log) {
            WeakListUtil.ForEach(_ChannelWatchers, (watcher) => {
                watcher.OnEvent(this, log);
            });
        }

        private WeakList<IEventWatcher<EventLog<TEvt>>> _ChannelWatchers = null;

        public int EventWatcherCount {
            get { return WeakListUtil.Count(_ChannelWatchers); }
        }

        public bool AddEventWatcher(IEventWatcher<EventLog<TEvt>> watcher) {
            return WeakListUtil.Add(ref _ChannelWatchers, watcher);
        }

        public bool RemoveEventWatcher(IEventWatcher<EventLog<TEvt>> watcher) {
            return WeakListUtil.Remove(_ChannelWatchers, watcher);
        }

        public void AddEventWatcher(IBlockOwner owner, Action<Aspect, EventLog<TEvt>> block) {
            AddEventWatcher(new BlockEventWatcher<EventLog<TEvt>>(owner, block));
        }
    }
}