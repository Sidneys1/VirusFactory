using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using VirusFactory.Model.Algorithm;
using VirusFactory.Model.Geography;
using static System.Console;

namespace VirusFactory.Console {
	class Program {
		static void Main() {
			var cs = Country.LoadCountries("countries.dat", "cities");

			Stats(cs);

			WriteLine();
			WriteLine();
			WriteLine();

			var running = true;

			do {
				Write("Enter a country: ");
				var str = ReadLine();
				Country selectedCountry = null;
				City[] cities = null;
				switch (str?.Trim().ToUpper() ?? "QUIT") {
					case "EXIT":
					case "QUIT":
						running = false;
						break;
					case "":
						selectedCountry = cs.Where(o => o.Cities.Count >= 2).RandomSubset(1).First();
						cities = selectedCountry.Cities.RandomSubset(2).ToArray();
						break;
					default:
						var str1 = str;
						selectedCountry = cs.Where(o => o.Name == str1).RandomSubset(1).First();
						if (selectedCountry == null) {
							WriteLine("Cound not find country, selecting random...");
							goto case "";
						}

						Write("Enter two cities, separated by ',' (e.g. New York,Philadelphia): ");
						str = ReadLine();
						switch ((str?.Trim() ?? "")) {
							case "":
								cities = selectedCountry.Cities.RandomSubset(2).ToArray();
								break;
							default:
								cities = str?.Split(',').Select(o => selectedCountry.Cities.FirstOrDefault(p => p.Name == o)).ToArray();
								if (cities == null || cities.Any(o => o == null)) {
									WriteLine("Cound not find a city, selecting random...");
									goto case "";
								}
								break;
						}

						break;
				}
				if (!running) continue;
				Debug.Assert(selectedCountry != null, "selectedCountry != null");
				Debug.Assert(cities != null, "cities != null");

				var path = AStar.FindPath(cities[0], cities[1], (city, city1) => {
					if (!city.Distances.ContainsKey(city1))
						city.Distances.Add(city1, Connection<City>.Distance(city, city1));

					return city.Distances[city1];
				}, city => {
					if (!city.Distances.ContainsKey(cities[1]))
						city.Distances.Add(cities[1], Connection<City>.Distance(city, cities[1]));

					return city.Distances[cities[1]] * 1.1;
				});

				//var path = DijkstraHelper<City>.CalculateShortestPathBetween(cities[0], cities[1], selectedCountry.Highways);
				WriteLine(
					$"Ron is travelling from {cities[0].Name} to {cities[1].Name} in {selectedCountry.Name}. His route is {path.TotalCost:#.##}km long:");
				WriteLine($"\t{path:#.#km}");
			} while (running);
		}

		private static void Stats(List<Country> cs) {
			WriteLine($"Island Nations:{Environment.NewLine}\t{cs.Count(o => o.BorderCountries.Count == 0)}");
			WriteLine($"Landlocked Nations:{Environment.NewLine}\t{cs.Count(o => !o.Ocean)}");
			WriteLine(
				$"Double Landlocked Nations:{Environment.NewLine}\t{cs.Where(o => o.BorderCountries.All(p => !p.Ocean)).Count(o => !o.Ocean)}");
			var selectedCountry = cs.MaxBy(o => o.Population);

			WriteLine();
			WriteLine($"Top Population:{Environment.NewLine}\t{selectedCountry.Name} with {selectedCountry.Population:N0} citizens");
			var avg = cs.Average(o => o.Population);
			WriteLine($"Average Population:{Environment.NewLine}\t{avg:#.##} citizens");
			selectedCountry = cs.MinBy(o => o.Population);
			WriteLine(
				$"Bottom Population:{Environment.NewLine}\t{selectedCountry.Name} with {selectedCountry.Population:N0} citizens");

			WriteLine();
			selectedCountry = cs.MaxBy(o => o.Density);
			WriteLine($"Top Population Density:{Environment.NewLine}\t{selectedCountry.Name} at {selectedCountry.Density:N0}/km^2");
			avg = cs.Average(o => o.Density);
			WriteLine($"Average Population Density:{Environment.NewLine}\t{avg:#.##}/km^2");
			selectedCountry = cs.MinBy(o => o.Density);
			WriteLine(
				$"Bottom Population Density:{Environment.NewLine}\t{selectedCountry.Name} at {selectedCountry.Density:N0}/km^2");
			selectedCountry = cs.MaxBy(o => o.GdpPerCapita);

			WriteLine();
			WriteLine(
				$"Top GDP Per Capita (Purchasing Power Parity):{Environment.NewLine}\t{selectedCountry.Name} at {selectedCountry.GdpPerCapita:C0}");
			avg = cs.Average(o => o.GdpPerCapita);
			WriteLine($"Average GDP Per Capita (Purchasing Power Parity):{Environment.NewLine}\t{avg:C0}");
			selectedCountry = cs.MinBy(o => o.GdpPerCapita);
			WriteLine(
				$"Bottom GDP Per Capita (Purchasing Power Parity):{Environment.NewLine}\t{selectedCountry.Name} at {selectedCountry.GdpPerCapita:C0}");
		}
	}
}
