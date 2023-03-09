using System;
using System.Collections.Generic;

using Edger.Unity;
using Edger.Unity.Weak;

namespace Edger.Unity.Weak {
    public abstract class BlockMono : BaseMono, IBlockOwner {
        private static int _NextIdentity = 0;
        private int _Identity = _NextIdentity++;
        public int Identity { get => _Identity; }

        public int Revision { get; private set; } 
        protected void AdvanceRevision() {
            Revision += 1;
        }

        public override string LogPrefix {
            get {
                return string.Format("{0} #{1:D5} @{2} ", base.LogPrefix, _Identity, Revision);
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
}
