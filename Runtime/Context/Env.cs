using System;
using System.Collections.Generic;

using UnityEngine;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    [DisallowMultipleComponent()]
    public class Env : BlockMono {
        private Dictionary<Type, Aspect> _Aspects = new Dictionary<Type, Aspect>();
        private Dictionary<Type, IAspectReference> _AspectCaches = new Dictionary<Type, IAspectReference>();

        public void ReloadAspects() {
            _Aspects.Clear();
            foreach (var cache in _AspectCaches.Values) {
                cache.Clear();
            }
            Aspect[] aspects = gameObject.GetComponents<Aspect>();
            for (int i = 0; i < aspects.Length; i++) {
                Aspect aspect = aspects[i];
                AddAspect(aspect);

                IAspectReference cache;
                if (_AspectCaches.TryGetValue(aspect.GetType(), out cache)) {
                    cache.SetTarget(aspect);
                }
            }
            AdvanceRevision();
        }

        public Aspect GetAspect(Type type) {
            if (_Aspects == null) {
                return null;
            }
            Aspect result;
            if (_Aspects.TryGetValue(type, out result)) {
                return result;
            }
            return null;
        }

        public T GetAspect<T>() where T : Aspect {
            Aspect result = GetAspect(typeof(T));
            if (result != null) {
                return result.As<T>();
            }
            return null;
        }

        private bool AddAspect(Aspect aspect) {
            if (aspect == null) return false;

            var old = GetAspect(aspect.GetType());
            if (old != null) {
                Error("Aspect Already Exist: <{0}> {1} -> {2}", aspect.GetType(), old, aspect);
                return false;
            }
            _Aspects[aspect.GetType()] = aspect;
            AdvanceRevision();
            return true;
        }

        public T AddAspect<T>() where T : Aspect {
            var old = GetAspect(typeof(T));
            if (old != null) {
                Error("Aspect Already Exist: <{0}> {1}", typeof(T), old);
                return null;
            }
            T result = gameObject.AddComponent<T>();
            _Aspects[typeof(T)] = result;
            AdvanceRevision();
            return result;
        }

        public T GetOrAddAspect<T>() where T : Aspect {
            T aspect = GetAspect<T>();
            if (aspect != null) {
                return aspect;
            }
            return AddAspect<T>();
        }

        public AspectReference<T> CacheAspect<T>() where T : Aspect {
            IAspectReference _cache;
            if (_AspectCaches.TryGetValue(typeof(T), out _cache)) {
                return _cache.As<AspectReference<T>>();
            } else {
                T aspect = GetOrAddAspect<T>();
                var cache = new AspectReference<T>(aspect);
                _AspectCaches[typeof(T)] = cache;
                return cache;
            }
        }
    }
}
