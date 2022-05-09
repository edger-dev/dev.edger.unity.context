using System;
using System.Collections.Generic;

namespace Edger.Unity.Weak {
    public class BlockOwner : IBlockOwner {
        private List<WeakBlock> _Blocks = null;

        public void AddBlock(WeakBlock block) {
            if (_Blocks == null) {
                _Blocks = new List<WeakBlock>();
            }
            if (!_Blocks.Contains(block)) {
                _Blocks.Add(block);
            }
        }

        public void RemoveBlock(WeakBlock block) {
            if (_Blocks == null) {
                return;
            }
            int index = _Blocks.IndexOf(block);
            if (index >= 0) {
                _Blocks.RemoveAt(index);
            }
        }
    }
}
