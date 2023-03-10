using System;
using System.Collections.Generic;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    public abstract class Aspect : BlockMono {
        public T GetEnv<T>() where T : Env {
            T env = gameObject.GetComponent<T>();
            if (env != null) {
                return env;
            } else {
                Error("GetEnv<{0}> Failed: not found", typeof(T).FullName);
                return null;
            }
        }

        public Env GetEnv() {
            return GetEnv<Env>();
        }

        public AspectReference<T> CacheAspect<T>() where T : Aspect {
            Env env = GetEnv<Env>();
            if (env != null) {
                return env.CacheAspect<T>();
            } else {
                return null;
            }
        }
    }

    public interface IAspectReference {
        public void _ClearReference();
        public bool _SetTarget(object target);
    }

    public class AspectReference<T> : IAspectReference where T : Aspect {
        public T Target { get; private set; }

        public AspectReference(T target) {
            Target = target;
        }

        public void _ClearReference() {
            Target = null;
        }

        public bool _SetTarget(object _target) {
            T target = _target.As<T>();
            if (target != null) {
                Target = target;
                return true;
            } else {
                return false;
            }
        }

        public void SetTarget(T target) {
            Target = target;
        }
    }

    public abstract class AspectLog {
        private static int _NextIdentity = 0;
        private int _Identity = _NextIdentity++;
        public int Identity { get => _Identity; }

        public readonly DateTime Time = DateTime.UtcNow;
        public readonly Type AspectType;

        public AspectLog(Aspect aspect) {
            AspectType = aspect.GetType();
        }
    }

    public interface IEventWatcher<TEvt> : IBlock {
        void OnEvent(Aspect aspect, TEvt evt);
    }

    public sealed class BlockEventWatcher<TEvt> : WeakBlock, IEventWatcher<TEvt> {
        private readonly Action<Aspect, TEvt> _Block;

        public BlockEventWatcher(IBlockOwner owner, Action<Aspect, TEvt> block) : base(owner) {
            _Block = block;
        }

        public void OnEvent(Aspect aspect, TEvt evt) {
            _Block(aspect, evt);
        }
    }
}