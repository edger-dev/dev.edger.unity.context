using System;
using System.Collections.Generic;

using UnityEngine;
using OneOf;

using Edger.Unity;
using Edger.Unity.Weak;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Edger.Unity.Context {
    public enum StatusCode : int {
        Ok = 200,
        Accepted= 202,

        InternalError = 500,
    }

    public sealed class HandleLog<TReq, TRes> : AspectLog {
        public readonly DateTime RequestTime;
        public readonly TReq Request;

        public DateTime ResponseTime { get => Time; }
        public readonly TRes Response;

        public readonly StatusCode StatusCode;
        public readonly Exception Error;

        public bool IsOk { get => StatusCode == StatusCode.Ok; }
        public bool IsAccepted { get => StatusCode == StatusCode.Accepted; }
        public bool IsError { get => Error != null; }

        public HandleLog(Handler<TReq, TRes> handler, DateTime reqTime, TReq req) : base(handler) {
            RequestTime = reqTime;
            Request = req;
            Response = default(TRes);
            StatusCode = StatusCode.Accepted;
        }

        public HandleLog(Handler<TReq, TRes> handler, DateTime reqTime, TReq req, TRes res) : base(handler) {
            RequestTime = reqTime;
            Request = req;
            Response = res;
            StatusCode = StatusCode.Ok;
        }

        public HandleLog(Handler<TReq, TRes> handler, DateTime reqTime, TReq req, StatusCode statusCode, Exception error) : base(handler) {
            RequestTime = reqTime;
            Request = req;
            Response = default(TRes);
            StatusCode = statusCode;
            Error = error;
        }

        public HandleLog(Handler<TReq, TRes> handler, DateTime reqTime, TReq req, StatusCode statusCode, string format, params object[] values)
            : this(handler, reqTime, req, statusCode, new EdgerException(format, values)) {
        }

        public override string ToString() {
            if (IsError) {
                return string.Format("[{0}] {1} -> {2}", StatusCode, Request, Error);
            } else if (IsOk) {
                return string.Format("[{0}] {1} -> {2}", StatusCode, Request, Response);
            } else {
                return string.Format("[{0}] {1}", StatusCode, Request);
            }
        }
    }

    public abstract class Handler<TReq, TRes> : Aspect {
        public Type RequestType { get => typeof(TReq); }
        public Type ResponseType { get => typeof(TRes); }

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        public HandleLog<TReq, TRes> Last { get; private set; }

        public HandleLog<TReq, TRes> HandleRequest(TReq req) {
            DateTime reqTime = DateTime.UtcNow;
            HandleLog<TReq, TRes> result = null;
            try {
                result = DoHandle(reqTime, req);
            } catch (Exception e) {
                result = new HandleLog<TReq, TRes>(this, reqTime, req, StatusCode.InternalError, e);
            }
            Last = result;
            AdvanceRevision();
            if (Last.IsError) {
                Error("HandleRequest Failed: {0}", Last);
            } else if (LogDebug) {
                Debug("HandleRequest: {0}", Last);
            }
            NotifyHandlerWatchers(Last);
            return Last;
        }

        protected abstract HandleLog<TReq, TRes> DoHandle(DateTime reqTime, TReq req);

        protected void NotifyHandlerWatchers(HandleLog<TReq, TRes> log) {
            WeakListUtil.ForEach(_HandlerWatchers, (watcher) => {
                watcher.OnEvent(this, log);
            });
        }

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        private WeakList<IEventWatcher<HandleLog<TReq, TRes>>> _HandlerWatchers = null;

        public int ResponseWatcherCount {
            get { return WeakListUtil.Count(_HandlerWatchers); }
        }

        public bool AddResponseWatcher(IEventWatcher<HandleLog<TReq, TRes>> watcher) {
            return WeakListUtil.Add(ref _HandlerWatchers, watcher);
        }

        public bool RemoveResponseWatcher(IEventWatcher<HandleLog<TReq, TRes>> watcher) {
            return WeakListUtil.Remove(_HandlerWatchers, watcher);
        }

        public void AddResponseWatcher(IBlockOwner owner, Action<Aspect, HandleLog<TReq, TRes>> block) {
            AddResponseWatcher(new BlockEventWatcher<HandleLog<TReq, TRes>>(owner, block));
        }
    }
}
