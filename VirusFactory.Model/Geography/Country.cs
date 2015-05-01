using System.Collections.Generic;
using ProtoBuf;
using VirusFactory.Model.Interface;

namespace VirusFactory.Model.Geography {
    [ProtoContract(AsReferenceDefault = true)]
	public class Country : IHasNeighbors<Country> {
        [ProtoMember(1)]
		public string Name { get; set; }
        [ProtoMember(2)]
        public int Population { get; set; }
        [ProtoMember(3)]
        public double Density { get; set; }
        [ProtoMember(4)]
        public double GdpPerCapita { get; set; }
        [ProtoMember(5)]
        public bool Ocean { get; set; }

        [ProtoMember(6, AsReference = true)]
        public List<Country> BorderCountries { get; } = new List<Country>();
        [ProtoMember(7, AsReference = true)]
        public List<City> Cities { get; } = new List<City>();
        [ProtoMember(8)]
        public Dictionary<Country, City> Outbound { get; } = new Dictionary<Country, City>();

		public IEnumerable<Country> Neighbors => BorderCountries;

		public override string ToString() => Name;
	}
}