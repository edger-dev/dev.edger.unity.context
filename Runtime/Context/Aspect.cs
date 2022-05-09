using System;
using System.Collections.Generic;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    public abstract class BaseAspect : BaseMono, IBlockOwner {
        private Env _Env = null;
        public Env Env {
            get {
                if (_Env == null) {
                    _Env = gameObject.GetOrAddComponent<Env>();
                }
                return _Env;
            }
        }

        private BlockOwner _BlockOwner = null;

        public void AddBlock(WeakBlock block) {
            if (_BlockOwner == null) {
                _BlockOwner = new BlockOwner();
            }
            _BlockOwner.AddBlock(block);
        }

        public void RemoveBlock(WeakBlock block) {
            if (_BlockOwner == null) {
                return;
            }
            _BlockOwner.RemoveBlock(block);
        }
    }

    public abstract class Aspect : BaseAspect {
        protected override void OnAwake() {
            Env.AddAspect(this);
        }
    }
}
