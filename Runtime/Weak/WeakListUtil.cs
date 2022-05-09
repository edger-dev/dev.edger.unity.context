using System;
using System.Collections.Generic;

namespace Edger.Unity.Weak {
    public static class WeakListUtil {
        public static int Count<T>(WeakList<T> list) where T : class {
            if (list != null) {
                return list.Count;
            }
            return 0;
        }

        public static bool Add<T>(ref WeakList<T> list, T element) where T : class {
            if (element == null) return false;
            if (list == null) {
                list = new WeakList<T>();
            }
            return list.Add(element);
        }

        public static bool Remove<T>(WeakList<T> list, T element) where T : class {
            if (element == null) return false;
            if (list != null) {
                return list.Remove(element);
            }
            return false;
        }

        public static void ForEach<T>(WeakList<T> list, Action<T> callback) where T : class {
            if (list != null) {
                list.ForEach(callback);
            }
        }
    }
}
