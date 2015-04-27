using System.Runtime.InteropServices;
using OpenTK;

namespace VirusFactory.OpenTK.VBOHelper {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct BufferElement {
		[FieldOffset(0)]
		public Vector2 Vertex;
		[FieldOffset(2 * sizeof(float))]
		public Vector4 Color;

		public BufferElement(Vector2 vertex, Vector4 color) {
			Vertex = vertex;
			Color = color;
		}

		public static readonly int SizeInBytes = Vector2.SizeInBytes + Vector4.SizeInBytes;
	}
}
