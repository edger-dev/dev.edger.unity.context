using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Edger.Unity;
using Edger.Unity.Weak;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Edger.Unity.Context {
    public abstract class CoroutineHandler<TReq, TRes> : Handler<TReq, TRes> {
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        public HandleLog<TReq, TRes> LastAsync { get; private set; }

        private Dictionary<int, IEnumerator> _RunningCoroutines = new Dictionary<int, IEnumerator>();
        public int RunningCount { get => _RunningCoroutines.Count; }

        private Dictionary<int, TRes> _Responses = new Dictionary<int, TRes>();

        public IEnumerator HandleRequestAsync(TReq req) {
            var log = new HandleLog<TReq, TRes>(this, DateTime.UtcNow, req);
            return DoHandleInternalAsync(log);
        }

        public void ClearRunningCoroutines() {
            foreach (var coroutine in _RunningCoroutines.Values) {
                StopCoroutine(coroutine);
            }
            _RunningCoroutines.Clear();
        }

        protected override HandleLog<TReq, TRes> DoHandle(DateTime reqTime, TReq req) {
            var log = new HandleLog<TReq, TRes>(this, reqTime, req);
            var coroutine = DoHandleInternalAsync(log);
            _RunningCoroutines[log.Identity] = coroutine;
            StartCoroutine(coroutine);
            return log;
        }

        private void OnAsyncResult(int reqIdentity, HandleLog<TReq, TRes> log) {
            if (_RunningCoroutines.ContainsKey(reqIdentity)) {
                _RunningCoroutines.Remove(reqIdentity);
            }
            if (_Responses.ContainsKey(reqIdentity)) {
                _Responses.Remove(reqIdentity);
            }
            LastAsync = log;
            AdvanceRevision();
            if (!LastAsync.IsOk) {
                Error("HandleRequestAsync Failed: {0}", LastAsync);
            } else if (LogDebug) {
                Debug("HandleRequestAsync: {0}", LastAsync);
            }
            NotifyHandlerWatchers(LastAsync);
        }

        private IEnumerator DoHandleInternalAsync(HandleLog<TReq, TRes> log) {
            if (_Responses.ContainsKey(log.Identity)) {
                _Responses.Remove(log.Identity);
            }
            yield return new WaitForEndOfFrame();
            HandleLog<TReq, TRes> result = null;
            IEnumerator handle = DoHandleAsync(log.Identity, log.RequestTime, log.Request);
            while (true) {
                try {
                    if (handle.MoveNext() == false) {
                        break;
                    }
                } catch (Exception e) {
                    result = new HandleLog<TReq, TRes>(this, log.RequestTime, log.Request, StatusCode.InternalError, e);
                    OnAsyncResult(log.Identity, result);
                    yield break;
                }
                yield return handle.Current;
            }
            TRes response = default(TRes);
            if (_Responses.TryGetValue(log.Identity, out response)) {
                result = new HandleLog<TReq, TRes>(this, log.RequestTime, log.Request, response);
            } else {
                result = new HandleLog<TReq, TRes>(this, log.RequestTime, log.Request, StatusCode.InternalError, "<{0}>.DoHandleAsync Failed: no response", GetType());
            }
            OnAsyncResult(log.Identity, result);
        }

        protected bool TryGetResponse(int reqIdentity, out TRes response) {
            return _Responses.TryGetValue(reqIdentity, out response);
        }

        protected void SetResponse(int reqIdentity, TRes response) {
            _Responses[reqIdentity] = response;
        }

        protected abstract IEnumerator DoHandleAsync(int reqIdentity, DateTime reqTime, TReq req);
    }
}
