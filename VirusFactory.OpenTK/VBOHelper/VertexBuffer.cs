using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace VirusFactory.OpenTK.VBOHelper {
	public class VertexBuffer {
		private int _id;

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

			GL.GenBuffers(1, out _id);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
			var ptr = new IntPtr(data.Length*Vector2.SizeInBytes);
			GL.BufferData(BufferTarget.ArrayBuffer, ptr, data, BufferUsageHint.StaticDraw);

			Length = data.Length;

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		public void Render() {
			
			GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);
			//GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
			//GL.VertexPointer(2, VertexPointerType.Float, 0, new IntPtr(0));
			GL.EnableVertexAttribArray(_id);
			GL.VertexAttribIPointer(0, 2, VertexAttribIntegerType.Int, Vector2.SizeInBytes, IntPtr.Zero);
			//GL.EnableClientState(ArrayCap.VertexArray);
			//GL.DrawArrays(PrimitiveType.Points, 0, Length);
			GL.PopClientAttrib();
		}
	}
}
