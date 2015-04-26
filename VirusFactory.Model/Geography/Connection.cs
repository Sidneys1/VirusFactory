using System;

namespace VirusFactory.Model.Geography
{
	public struct Connection<T> where T : ICoordinate
	{
		public readonly T LocationA;
		public readonly T LocationB;
		public double Length;

		public Connection(T locationA, T locationB)
		{
			LocationA = locationA;
			LocationB = locationB;

			Length = Distance(LocationA.Latitude, locationA.Longitude, locationB.Latitude, locationB.Longitude);
		}

		public static double Distance(double lat1, double lon1, double lat2, double lon2)
		{
			var theta = lon1 - lon2;
			var dist = Math.Sin(Deg2Rad(lat1))*Math.Sin(Deg2Rad(lat2)) +
			           Math.Cos(Deg2Rad(lat1))*Math.Cos(Deg2Rad(lat2))*Math.Cos(Deg2Rad(theta));

			dist = Math.Acos(dist);
			dist = Rad2Deg(dist);
			dist *= 111.18957696; // Magic number whoooo. Convters to km;
            return dist;
		}

		public static double Distance(Point p1, Point p2)
		{
			return Distance(p1.Y, p1.X, p2.Y, p2.X);
		}

		private static double Deg2Rad(double deg)
		{
			return deg * Math.PI / 180.0;
		}

		private static double Rad2Deg(double rad)
		{
			return rad/Math.PI*180.0;
		}

		public override int GetHashCode()
		{
			return LocationA.GetHashCode() ^ LocationB.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Connection<T>))
				return false;
			var o = (Connection<T>) obj;

			return ((o.LocationA.Equals(LocationA) && (o.LocationB.Equals(LocationB))) ||
			        (o.LocationA.Equals(LocationB) && o.LocationB.Equals(LocationA)));
		}

		public override string ToString() => $"{LocationA} -> {LocationB}";
	}
}
