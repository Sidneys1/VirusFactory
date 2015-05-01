using System.Collections.Generic;
using MIConvexHull;
using ProtoBuf;
using VirusFactory.Model.Interface;

namespace VirusFactory.Model.Geography
{
    [ProtoContract(AsReferenceDefault = true)]
    public class City : ICoordinate, IHasNeighbors<City>, IVertex
    {
        [ProtoMember(1)]
        public Point Point { get; set; }

        public City(string name, Country country, double latitude, double longitude)
        {
            Name = name;
            Point = new Point(longitude, latitude);
            Country = country;
        }

        public City() {
        }

        [ProtoMember(2)]
        public string Name { get; set; }
        public double Latitude => Point.Y;
        public double Longitude => Point.X;
        [ProtoMember(3)]
        public bool IsHull { get; set; } = false;
        [ProtoMember(4, AsReference = true)]
        public Country Country { get; set; }

        [ProtoMember(5, AsReference = true)]
        public List<City> BorderCities { get; } = new List<City>();
        [ProtoMember(6)]
        public Dictionary<City, double> Distances { get; } = new Dictionary<City, double>();

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is City))
                return false;

            var o = (City)obj;
            return Point.Equals(o.Point);
        }

        public IEnumerable<City> Neighbors => BorderCities;

        public override string ToString() => Name;
        public double[] Position => new[] { Point.X, Point.Y };
    }
}
