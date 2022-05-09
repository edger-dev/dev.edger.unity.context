using System;
using System.Collections.Generic;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    public class Env : BaseMono {
        private Dictionary<Type, Aspect> _Aspects = null;

        public void ReloadAspects() {
            if (_Aspects != null) {
                _Aspects.Clear();
            }
            Aspect[] aspects = gameObject.GetComponents<Aspect>();
            for (int i = 0; i < aspects.Length; i++) {
                Aspect aspect = aspects[i];
                AddAspect(aspect);
            }
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

        public bool AddAspect(Aspect aspect) {
            if (aspect == null) return false;

            var old = GetAspect(aspect.GetType());
            if (old != null) {
                Error("Aspect Already Exist: <{0}> {1} -> {2}", aspect.GetType(), old, aspect);
                return false;
            }
            if (_Aspects == null) {
                _Aspects = new Dictionary<Type, Aspect>();
            }
            _Aspects[aspect.GetType()] = aspect;
            return true;
        }

        public T AddAspect<T>() where T : Aspect {
            var old = GetAspect(typeof(T));
            if (old != null) {
                Error("Aspect Already Exist: <{0}> {1}", typeof(T), old);
                return null;
            }
            if (_Aspects == null) {
                _Aspects = new Dictionary<Type, Aspect>();
            }
            T result = gameObject.AddComponent<T>();
            _Aspects[typeof(T)] = result;
            return result;
        }

        public T GetOrAddAspect<T>() where T : Aspect {
            T aspect = GetAspect<T>();
            if (aspect != null) {
                return aspect;
            }
            return AddAspect<T>();
        }
    }
}
