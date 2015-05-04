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

using MIConvexHull.ConvexHull.Algorithm;
using MIConvexHull.ConvexHull.Collections;
using System;

namespace MIConvexHull.ConvexHull {

    /// <summary>
    /// A helper class for object allocation/storage.
    /// This helps the GC a lot as it prevents the creation of about 75% of
    /// new face objects (in the case of ConvexFaceInternal). In the case of
    /// FaceConnectors and DefferedFaces, the difference is even higher (in most
    /// cases O(1) vs O(number of created faces)).
    /// </summary>
    internal class ObjectManager {
        private readonly int _dimension;

        private readonly ConvexHullInternal _hull;
        private int _facePoolSize, _facePoolCapacity;
        private ConvexFaceInternal[] _facePool;
        private readonly IndexBuffer _freeFaceIndices;
        private FaceConnector _connectorStack;
        private readonly SimpleList<IndexBuffer> _emptyBufferStack;
        private readonly SimpleList<DeferredFace> _deferredFaceStack;

        /// <summary>
        /// Return the face to the pool for later use.
        /// </summary>
        /// <param name="faceIndex"></param>
        public void DepositFace(int faceIndex) {
            var face = _facePool[faceIndex];
            var af = face.AdjacentFaces;
            for (var i = 0; i < af.Length; i++) {
                af[i] = -1;
            }
            _freeFaceIndices.Push(faceIndex);
        }

        /// <summary>
        /// Reallocate the face pool, including the AffectedFaceFlags
        /// </summary>
        private void ReallocateFacePool() {
            var newPool = new ConvexFaceInternal[2 * _facePoolCapacity];
            var newTags = new bool[2 * _facePoolCapacity];
            Array.Copy(_facePool, newPool, _facePoolCapacity);
            Buffer.BlockCopy(_hull.AffectedFaceFlags, 0, newTags, 0, _facePoolCapacity * sizeof(bool));
            _facePoolCapacity = 2 * _facePoolCapacity;
            _hull.FacePool = newPool;
            _facePool = newPool;
            _hull.AffectedFaceFlags = newTags;
        }

        /// <summary>
        /// Create a new face and put it in the pool.
        /// </summary>
        /// <returns></returns>
        private int CreateFace() {
            var index = _facePoolSize;
            var face = new ConvexFaceInternal(_dimension, index, GetVertexBuffer());
            _facePoolSize++;
            if (_facePoolSize > _facePoolCapacity) ReallocateFacePool();
            _facePool[index] = face;
            return index;
        }

        /// <summary>
        /// Return index of an unused face or creates a new one.
        /// </summary>
        /// <returns></returns>
        public int GetFace() {
            if (_freeFaceIndices.Count > 0) return _freeFaceIndices.Pop();
            return CreateFace();
        }

        /// <summary>
        /// Store a face connector in the "embedded" linked list.
        /// </summary>
        /// <param name="connector"></param>
        public void DepositConnector(FaceConnector connector) {
            if (_connectorStack == null) {
                connector.Next = null;
                _connectorStack = connector;
            } else {
                connector.Next = _connectorStack;
                _connectorStack = connector;
            }
        }

        /// <summary>
        /// Get an unused face connector. If none is available, create it.
        /// </summary>
        /// <returns></returns>
        public FaceConnector GetConnector() {
            if (_connectorStack == null) return new FaceConnector(_dimension);

            var ret = _connectorStack;
            _connectorStack = _connectorStack.Next;
            ret.Next = null;
            return ret;
        }

        /// <summary>
        /// Deposit the index buffer.
        /// </summary>
        /// <param name="buffer"></param>
        public void DepositVertexBuffer(IndexBuffer buffer) {
            buffer.Clear();
            _emptyBufferStack.Push(buffer);
        }

        /// <summary>
        /// Get a store index buffer or create a new instance.
        /// </summary>
        /// <returns></returns>
        public IndexBuffer GetVertexBuffer() {
            return _emptyBufferStack.Count != 0 ? _emptyBufferStack.Pop() : new IndexBuffer();
        }

        /// <summary>
        /// Deposit the deferred face.
        /// </summary>
        /// <param name="face"></param>
        public void DepositDeferredFace(DeferredFace face) {
            _deferredFaceStack.Push(face);
        }

        /// <summary>
        /// Get the deferred face.
        /// </summary>
        /// <returns></returns>
        public DeferredFace GetDeferredFace() {
            return _deferredFaceStack.Count != 0 ? _deferredFaceStack.Pop() : new DeferredFace();
        }

        /// <summary>
        /// Create the manager.
        /// </summary>
        /// <param name="hull"></param>
        public ObjectManager(ConvexHullInternal hull) {
            _dimension = hull.Dimension;
            _hull = hull;
            _facePool = hull.FacePool;
            _facePoolSize = 0;
            _facePoolCapacity = hull.FacePool.Length;
            _freeFaceIndices = new IndexBuffer();

            _emptyBufferStack = new SimpleList<IndexBuffer>();
            _deferredFaceStack = new SimpleList<DeferredFace>();
        }
    }
}