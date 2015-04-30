using System.Collections.Generic;
using VirusFactory.Model.Interface;

namespace VirusFactory.Model.Geography {
	public class Country : IHasNeighbors<Country> {
		public string Name { get; set; }
		public int Population { get; set; }
		public double Density { get; set; }
		public double GdpPerCapita { get; set; }
		public bool Ocean { get; set; }

		public List<Country> BorderCountries { get; } = new List<Country>();
		public List<City> Cities { get; } = new List<City>();
		public Dictionary<Country, City> Outbound { get; } = new Dictionary<Country, City>();

		public IEnumerable<Country> Neighbors => BorderCountries;

		public override string ToString() => Name;
	}
}