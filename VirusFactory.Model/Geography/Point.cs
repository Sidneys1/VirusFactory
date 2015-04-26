// ReSharper disable CompareOfFloatsByEqualityOperator

namespace VirusFactory.Model.Geography
{
	public struct Point
	{
		public Point(double x, double y)
		{
			Y = y;
			X = x;
		}

		public double Y { get; }
		public double X { get; }

		public override string ToString() => $"({X},{Y})";

		public override bool Equals(object obj)
		{
			if (!(obj is Point)) return false;
			var p = (Point) obj;
			
			return (X == p.X) && (Y == p.Y);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}
	}
}