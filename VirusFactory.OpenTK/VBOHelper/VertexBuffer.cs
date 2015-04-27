using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace VirusFactory.OpenTK.VBOHelper {
	public class VertexBuffer {
		private readonly int _vboId;

		public VertexBuffer(BufferElement[] data) {
			if (data == null) return;

			GL.GenBuffers(1, out _vboId);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
			GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(data.Length * BlittableValueType.StrideOf(data)), data, BufferUsageHint.DynamicDraw);
			
			Length = data.Length;
		}
		
		public int Length { get; }

		public int Id => _vboId;

		public void Render(PrimitiveType primitiveType = PrimitiveType.Points) {
			GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.ColorArray);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
			GL.VertexPointer(2, VertexPointerType.Float, BufferElement.SizeInBytes, new IntPtr(0));
			GL.ColorPointer(4, ColorPointerType.Float, BufferElement.SizeInBytes, new IntPtr(Vector2.SizeInBytes));
			GL.DrawArrays(primitiveType, 0, Length);
		}
	}
}

