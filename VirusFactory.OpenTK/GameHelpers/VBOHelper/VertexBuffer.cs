using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace VirusFactory.OpenTK.GameHelpers.VBOHelper {

    public class VertexBuffer<T> : IDisposable where T : struct {

        #region Fields

        private readonly int _id;
        private readonly Action _prep;

        #endregion Fields

        #region Properties

        public bool IsDisposed { get; private set; }

        public int Length { get; }

        public int Id => _id;

        #endregion Properties

        #region Constructors

        public VertexBuffer(T[] data, Action prep, BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw) {
            if (data == null) return;

            GL.GenBuffers(1, out _id);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(data.Length * BlittableValueType.StrideOf(data)), data, bufferUsageHint);
            Length = data.Length;
            _prep = prep;
        }

        #endregion Constructors

        #region Destructors

        ~VertexBuffer() {
            Dispose();
        }

        #endregion Destructors

        #region Methods

        public void Render(PrimitiveType primitiveType = PrimitiveType.Points) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _id);

            _prep.Invoke();

            GL.DrawArrays(primitiveType, 0, Length);
        }

        public void Dispose() {
            if (IsDisposed) return;
            try {
                GL.DeleteBuffer(_id);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            IsDisposed = true;
        }

        #endregion Methods
    }
}