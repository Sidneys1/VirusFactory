// ReSharper disable CompareOfFloatsByEqualityOperator

namespace VirusFactory.Model.Geography
{
	//[StructLayout(LayoutKind.Explicit)]
	public struct Point
	{
		public Point(double x, double y) {
			Y = y;
			X = x;
		}
		//[FieldOffset(0)]
		public readonly double X;
		//[FieldOffset(sizeof(double))]
		public readonly double Y;
		//[FieldOffset(0)]
		//public fixed double Position[2];
		
		public override string ToString() => $"({X},{Y})";

		public override bool Equals(object obj) {
			if (!(obj is Point)) return false;
			var p = (Point)obj;

			return (X == p.X) && (Y == p.Y);
		}

		public override int GetHashCode() {
			return X.GetHashCode() ^ Y.GetHashCode();
		}
	}
}