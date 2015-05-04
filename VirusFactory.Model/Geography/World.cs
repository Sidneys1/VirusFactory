using FortuneVoronoi;
using MIConvexHull.ConvexHull;
using MoreLinq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VirusFactory.Model.Geography {

    [ProtoContract(SkipConstructor = true)]
    public class World {

        [ProtoMember(1, AsReference = false)]
        private readonly List<Country> _countries = new List<Country>();

        public IReadOnlyList<Country> Countries => _countries;

        public World(string countriesFile, string citiesFolder) {
            var bordersTemp = new Dictionary<string, string[]>();
            var missing = new List<string>();

            using (var s = File.OpenText(countriesFile)) {
                while (!s.EndOfStream) {
                    var l = s.ReadLine();

                    var sections = l?.Split('\t');

                    var c = new Country();
                    if (sections == null) continue;
                    c.Name = sections[0];

                    var stats = sections[1].Split(',');
                    c.Population = int.Parse(stats[0]);
                    c.Density = double.Parse(stats[1]);
                    c.GdpPerCapita = double.Parse(stats[2]);

                    stats = sections[2].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (bordersTemp.ContainsKey(c.Name)) {
                        Console.WriteLine($"Possible Duplicate: {c.Name}.");
                    } else if (stats.Any())
                        bordersTemp.Add(c.Name, stats);

                    c.Ocean = bool.Parse(sections[3]);

                    _countries.Add(c);
                }
            }
            foreach (var country in _countries) {
                var highwaysTemp = new Dictionary<City, string[]>();
                //Load cities
                if (File.Exists($".\\{citiesFolder}\\{country.Name}.dat")) {
                    using (var s = File.OpenText($".\\{citiesFolder}\\{country.Name}.dat")) {
                        while (!s.EndOfStream) {
                            var str = s.ReadLine()?.Split('\t');
                            if (country.Cities.Any(o => o.Name == str?[0])) continue;
                            var coor = str?[1].Split(',').Select(double.Parse).ToArray() ?? new[] { 0.0, 0.0 };
                            var c = new City(str?[0], country, coor[0], coor[1]);
                            highwaysTemp.Add(c, str?[2].Split(','));
                            country.Cities.Add(c);
                        }
                    }

                    foreach (var city in country.Cities) {
                        if (!highwaysTemp.ContainsKey(city)) continue;
                        foreach (var s in highwaysTemp[city].Where(s => s != city.Name)) {
                            if (country.Cities.Count(o => o.Name == s) == 0)
                                Console.WriteLine($"Missing city: '{s}'.");
                            else
                                city.BorderCities.Add(country.Cities.First(o => o.Name == s));
                        }
                    }
                } else
                    Console.WriteLine($"Missing city file {country.Name}.dat");

                _countries.AsParallel().Where(o => o.Cities.Count > 0).SelectMany(o => ConvexHull.Create(o.Cities).Points).ForEach(o => o.IsHull = true);

                //Load borders
                if (!bordersTemp.ContainsKey(country.Name)) continue;
                foreach (var s in bordersTemp[country.Name]) {
                    if (_countries.Count(o => o.Name == s) == 0) {
                        if (!missing.Contains(s))
                            missing.Add(s);
                        country.BorderCountries.Add(new Country { Name = s });
                    } else
                        country.BorderCountries.AddRange(_countries.Where(o => o.Name == s));
                }
            }

            // Outbound
            foreach (var source in _countries.Where(o => o.Cities.Count != 0 && o.BorderCountries.Count != 0)) {
                foreach (var source1 in source.BorderCountries.Where(o => o.Cities.Count != 0 && !source.Outbound.ContainsKey(o))) {
                    var graph = Fortune.ComputeVoronoiGraph(source.Cities.Union(source1.Cities).Where(o => o.IsHull).Select(o => new Vector(o.Position) { Tag = o }));
                    var voronoiEdge = graph.Edges.Where(o => ((City)o.LeftData.Tag).Country != ((City)o.RightData.Tag).Country)
                        .MinBy(o => Connection<City>.Distance((City)o.LeftData.Tag, (City)o.RightData.Tag));
                    var rightCity = (City)voronoiEdge.RightData.Tag;
                    var leftCity = (City)voronoiEdge.LeftData.Tag;
                    source.Outbound[source1] = leftCity.Country != source ? rightCity : leftCity;
                    source1.Outbound[source] = rightCity.Country != source1 ? leftCity : rightCity;
                }
            }
        }

        public static void Save(World w, string path) {
            using (var file = File.Create(path)) {
                Serializer.Serialize(file, w);
            }
        }

        public static World Load(string path) {
            using (var file = File.OpenRead(path)) {
                return Serializer.Deserialize<World>(file);
            }
        }
    }
}