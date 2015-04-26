using System.Collections.Generic;
using MIConvexHull;
using VirusFactory.Model.Interface;

namespace VirusFactory.Model.Geography
{
	public class City : ICoordinate, IHasNeighbors<City>, IVertex
	{
		public Point Point { get; }

		public City(string name, Country country, double latitude, double longitude)
		{
			Name = name;
			Point = new Point(longitude, latitude);
			Country = country;
		}

		public string Name { get; }
		public double Latitude => Point.Y;

		public double Longitude => Point.X;

		public bool IsHull { get; set; } = false;

		public Country Country { get; }

		//public List<Connection<City>> Highways { get; } = new List<Connection<City>>();
		public List<City> BorderCities { get; } = new List<City>();
		public Dictionary<City, double> Distances { get; } = new Dictionary<City, double>();

		public override int GetHashCode()
		{
			return Name.GetHashCode() ^ Latitude.GetHashCode() ^ Longitude.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is City))
				return false;

			var o = (City) obj;
			return Point.Equals(o.Point);
		}

		public IEnumerable<City> Neighbors => BorderCities;

		public override string ToString() => Name;
		public double[] Position => new[] {Point.X, Point.Y};
	}
}
