using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using VirusFactory.Model.Geography;

// ReSharper disable AccessToDisposedClosure

namespace VirusFactory.OpenTK {
	class Program {
		private static Country _selectedCountry;
		private static City[] _cities;
		private static Path<City> _calculateShortestPathBetween;
		private static double[][] _points;
		private static int _frame;
		static void Main() {
			var cs = Country.LoadCountries("countries.dat", "cities");
			_points = cs.SelectMany(o => o.Cities).Select(o => new[] { o.Latitude, o.Longitude }).ToArray();

			var viableCountries = cs.Where(o => o.Cities.Count >= 2);

			var maxLat = _points.Max(o => o[0]);
			var maxLon = _points.Max(o => o[1]);

			var minLat = _points.Min(o => o[0]);
			var minLon = _points.Min(o => o[1]);

			var scaleLat = Math.Abs(maxLat - minLat) / 2;
			var scaleLon = Math.Abs(maxLon - minLon) / 2;

			minLat = _points.Min(o => o[0]) / scaleLat;
			minLon = _points.Min(o => o[1]) / scaleLon;

			var addLat = -minLat - 1;
			var addLon = -minLon - 1;
			addLon -= 0.2;

			_points = _points.Select(o => {
				var d = (o[1] / scaleLon) + addLon;
				if (d < -1)
					d = 2 + d;
				return new[] { d, (o[0] / scaleLat) + addLat };
			}).ToArray();

			var highways = cs.SelectMany(o => o.Cities.SelectMany(p => p.BorderCities.Select(q => new Connection<City>(p, q)))).Distinct().ToArray();

			using (var game = new GameWindow(1280, 720, new GraphicsMode(32, 24, 0, 4))) {
				game.Load += (sender, e) => {
					game.VSync = VSyncMode.Off;
					GL.PointSize(2);

					GL.ClearColor(0f, 0f, 0f, 1f);
				};

				//_selectedCountry = cs.First(o => o.Name == "United States");
				//_cities = new[] {
				//	_selectedCountry.Cities.First(o=>o.Name=="New York City"),
				//	_selectedCountry.Cities.First(o=>o.Name=="Los Angeles"),
				//};
				//_calculateShortestPathBetween = AStar.FindPath(_cities[0], _cities[1], Distance, Estimate);
				game.UpdateFrame += (sender, args) => {
					_selectedCountry = viableCountries.RandomSubset(1).First();
					_cities = _selectedCountry?.Cities.RandomSubset(2).ToArray();
					if (_cities != null && _selectedCountry != null)
						_calculateShortestPathBetween = AStar.FindPath(_cities[0], _cities[1], Distance, Estimate);
				};

				game.Resize += (sender, e) => {
					GL.Viewport(0, 0, game.Width, game.Height);
				};
				game.KeyDown += (o, eventArgs) => {
					if (eventArgs.Key == Key.Escape)
						game.Exit();
				};

				game.RenderFrame += (sender, e) => {
					// render graphics
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

					GL.MatrixMode(MatrixMode.Projection);
					GL.LoadIdentity();
					GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
					GL.Begin(PrimitiveType.Lines);
					GL.LineWidth(0.5f);
					for (var i = 0; i < highways.Length; i++) {
						var d = (highways[i].LocationA.Longitude / scaleLon) + addLon;
						if (d < -1)
							d = 2 + d;
						var d1 = (highways[i].LocationB.Longitude / scaleLon) + addLon;
						if (d1 < -1)
							d1 = 2 + d1;

						GL.Color3(0.1f, 0.1f, 0.1f);
						GL.Vertex2(d, (highways[i].LocationA.Latitude / scaleLat) + addLat);
						GL.Vertex2(d1, (highways[i].LocationB.Latitude / scaleLat) + addLat);
					}
					GL.End();
					GL.Begin(PrimitiveType.Points);
					for (var i = 0; i < _points.Length; i++) {
						GL.Color3(0.2f, 0.2f, 0.2f);
						GL.Vertex2(_points[i]);
					}
					GL.End();
					GL.Begin(PrimitiveType.LineStrip);
					GL.LineWidth(2f);
					if (_calculateShortestPathBetween != null)
						foreach (var connection in _calculateShortestPathBetween) {
							var d = (connection.Longitude / scaleLon) + addLon;
							if (d < -1)
								d = 2 + d;

							GL.Color3(1f, 0f, 0f);
							GL.Vertex2(d, (connection.Latitude / scaleLat) + addLat);
							//GL.Vertex2(d1, (connection.LocationB.Latitude / scaleLat) + addLat);
						}
					GL.End();

					game.SwapBuffers();

					if ((_frame++) % 60 == 0) {
						game.Title =
							$"TPS: {Math.Ceiling(game.UpdateFrequency):000}, FPS: {Math.Ceiling(game.RenderFrequency):000}";
						_frame = 1;
					}


				};

				// Run the game at 60 updates per second
				game.Run(60, 60);
			}
		}

		private static double Estimate(City city)
		{
			if (!city.Distances.ContainsKey(_cities[1]))
				city.Distances.Add(_cities[1], Connection<City>.Distance(city.Point, _cities[1].Point));

			return city.Distances[_cities[1]]*1.1;
		}

		private static double Distance(City city, City city1) {
			if (!city.Distances.ContainsKey(city1))
				city.Distances.Add(city1, Connection<City>.Distance(city.Point, city1.Point));

			return city.Distances[city1];
		}
	}
}

