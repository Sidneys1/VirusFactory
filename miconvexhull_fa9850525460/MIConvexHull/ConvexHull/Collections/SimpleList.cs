/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2014 David Sehnal, Matthew Campbell
 *
 *  This library is free software; you can redistribute it and/or modify it
 *  under the terms of  the GNU Lesser General Public License as published by
 *  the Free Software Foundation; either version 2.1 of the License, or
 *  (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser
 *  General Public License for more details.
 *
 *****************************************************************************/

using System;

namespace MIConvexHull.ConvexHull.Collections {

    /// <summary>
    /// A more lightweight alternative to List of T.
    /// On clear, only resets the count and does not clear the references
    ///   => this works because of the ObjectManager.
    /// Includes a stack functionality.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SimpleList<T> {
        private T[] _items;
        private int _capacity;

        public int Count;

        /// <summary>
        /// Get the i-th element.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i] {
            get { return _items[i]; }
            set { _items[i] = value; }
        }

        /// <summary>
        /// Size matters.
        /// </summary>
        private void EnsureCapacity() {
            if (_capacity == 0) {
                _capacity = 32;
                _items = new T[32];
            } else {
                var newItems = new T[_capacity * 2];
                Array.Copy(_items, newItems, _capacity);
                _capacity = 2 * _capacity;
                _items = newItems;
            }
        }

        /// <summary>
        /// Adds a vertex to the buffer.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item) {
            if (Count + 1 > _capacity) EnsureCapacity();
            _items[Count++] = item;
        }

        /// <summary>
        /// Pushes the value to the back of the list.
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item) {
            if (Count + 1 > _capacity) EnsureCapacity();
            _items[Count++] = item;
        }

        /// <summary>
        /// Pops the last value from the list.
        /// </summary>
        /// <returns></returns>
        public T Pop() {
            return _items[--Count];
        }

        /// <summary>
        /// Sets the Count to 0, otherwise does nothing.
        /// </summary>
        public void Clear() {
            Count = 0;
        }
    }
}