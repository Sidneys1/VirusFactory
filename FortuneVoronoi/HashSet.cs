using System;
using System.Collections.Generic;

namespace FortuneVoronoi {

    /// <summary>
    /// Set für effizienten Zugriff auf Objekte. Objete werden als Key abgelegt, value ist nur ein dummy-Objekt.
    /// </summary>
    [Serializable]
    public class HashSet<T> : ICollection<T> {
        private readonly Dictionary<T, object> _core;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object Dummy = new object();

        public HashSet(IEnumerable<T> source)
            : this() {
            AddRange(source);
        }

        public HashSet(IEqualityComparer<T> eqComp) {
            _core = new Dictionary<T, object>(eqComp);
        }

        public HashSet() {
            _core = new Dictionary<T, object>();
        }

        public bool Add(T o) {
            var count = _core.Count;
            _core[o] = Dummy;
            return count != _core.Count;
        }

        public bool Contains(T o) {
            return _core.ContainsKey(o);
        }

        public bool Remove(T o) {
            return _core.Remove(o);
        }

        [Obsolete]
        public void AddRange(System.Collections.IEnumerable list) {
            foreach (T o in list)
                Add(o);
        }

        public void AddRange(IEnumerable<T> list) {
            foreach (var o in list)
                Add(o);
        }

        public void Clear() {
            _core.Clear();
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            return _core.Keys.GetEnumerator();
        }

        #endregion IEnumerable<T> Members

        #region ICollection<T> Members

        public bool IsSynchronized => false;

        public int Count => _core.Count;

        public void CopyTo(T[] array, int index) {
            _core.Keys.CopyTo(array, index);
        }

        public bool IsReadOnly => false;

        #endregion ICollection<T> Members

        void ICollection<T>.Add(T item) {
            Add(item);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _core.Keys.GetEnumerator();
        }
    }
}