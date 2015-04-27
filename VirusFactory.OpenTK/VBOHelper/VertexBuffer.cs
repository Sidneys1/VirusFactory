using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace VirusFactory.OpenTK.VBOHelper {
	public class VertexBuffer {
		private readonly int[] _vboId = new int[2];

		public VertexBuffer() {
			// Original Constructor code. Removed because it doesn't account for multi-element buffers?
			//GraphicsContext.Assert();
			//GL.GenBuffers(1, out _vertexBuffer);
			//if (_vertexBuffer == 0)
			//	throw new Exception("Could not create VBO.");
		}
		
		public int Length { get; private set; }
		
		public void SetData(Vector2[] data) {
			if (data == null) return;

			GL.GenBuffers(2, _vboId);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId[0]);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vboId[1]);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vboId[1]);
			var v = Enumerable.Range(0, data.Length).ToArray();
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(data.Length * sizeof(ushort)), v, BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId[0]);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * 8*sizeof(float)), data, BufferUsageHint.StaticDraw);
			
			//var ptr = new IntPtr(data.Length*Vector2.SizeInBytes);
			//GL.BufferData(BufferTarget.ArrayBuffer, ptr, data, BufferUsageHint.StaticDraw);

			//GL.EnableClientState(ArrayCap.VertexArray);
			GL.VertexPointer(2, VertexPointerType.Float, 8* sizeof(float), IntPtr.Zero);

			Length = data.Length;

			GL.BindVertexArray(0);
		}

		public void Render() {
			
			//GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);
			////GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
			////GL.VertexPointer(2, VertexPointerType.Float, 0, new IntPtr(0));
			//GL.EnableVertexAttribArray(_vboId);
			//GL.VertexAttribIPointer(0, 2, VertexAttribIntegerType.Int, Vector2.SizeInBytes, IntPtr.Zero);
			////GL.EnableClientState(ArrayCap.VertexArray);
			////GL.DrawArrays(PrimitiveType.Points, 0, Length);
			//GL.PopClientAttrib();
			
			GL.BindVertexArray(_vboId[0]);
			//GL.VertexPointer(2, VertexPointerType.Int, 8*sizeof(float), IntPtr.Zero);
			GL.DrawElements(PrimitiveType.Points, Length, DrawElementsType.UnsignedInt, Enumerable.Range(0,Length).ToArray());
			//GL.DrawArrays(PrimitiveType.Points, 0, Length);
			//GL.DrawRangeElements(PrimitiveType.Points, 0, Length, Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
			//GL.BindVertexArray(0);
		}
	}
}

