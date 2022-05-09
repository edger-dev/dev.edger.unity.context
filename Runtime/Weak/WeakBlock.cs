using System;
using System.Collections.Generic;

namespace Edger.Unity.Weak {
    public interface IBlock {
        string BlockName { get; }
    }

    public interface IBlockOwner {
        void AddBlock(WeakBlock block);
        void RemoveBlock(WeakBlock block);
    }

    public abstract class WeakBlock : IBlock {
        private readonly WeakReference _OwnerReference = null;

        public bool IsOwnerAlive {
            get {
                return _OwnerReference != null && _OwnerReference.IsAlive;
            }
        }

        protected WeakBlock(IBlockOwner owner) {
            if (owner != null) {
                _OwnerReference = new WeakReference(owner);
            }
        }

        public override string ToString() {
            return BlockName;
        }

        public string BlockName {
            get {
                return (IsOwnerAlive ? "" : "!") + GetType().Name;
            }
        }

        public void OnAdded() {
            if (IsOwnerAlive) {
                ((IBlockOwner)_OwnerReference.Target).AddBlock(this);
            }
        }

        public void OnRemoved() {
            if (IsOwnerAlive) {
                ((IBlockOwner)_OwnerReference.Target).RemoveBlock(this);
            }
        }
    }
}
