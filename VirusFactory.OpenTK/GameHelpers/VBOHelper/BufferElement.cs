using System.Runtime.InteropServices;
using OpenTK;

namespace VirusFactory.OpenTK.GameHelpers.VBOHelper {
	[StructLayout(LayoutKind.Explicit)]
	public struct BufferElement {
		[FieldOffset(0)]
		public Vector2 Vertex;
		[FieldOffset(2 * sizeof(float))]
		public Vector4 Color;
		[FieldOffset(6 * sizeof(float))]
		public float OriginalW;
		public BufferElement(Vector2 vertex, Vector4 color) {
			Vertex = vertex;
			Color = color;
			OriginalW = Color.W;
		}

		public static readonly int SizeInBytes = Vector2.SizeInBytes + Vector4.SizeInBytes + sizeof(float);
	}
}
