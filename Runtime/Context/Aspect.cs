using System;
using System.Collections.Generic;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Context {
    public abstract class Aspect : BaseMono, IBlockOwner {
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
}
