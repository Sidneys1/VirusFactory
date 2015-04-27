using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using VirusFactory.Model.Algorithm;
using VirusFactory.Model.Geography;
using QuickFont;
using VirusFactory.OpenTK.VBOHelper;

// ReSharper disable PossibleUnintendedReferenceComparison

// ReSharper disable AccessToDisposedClosure

namespace VirusFactory.OpenTK {
	class Program {
		//private static Country _selectedCountry;
		private static City[] _pathingCities;
		//private static Path<City> _calculateShortestPathBetween;
		private static Vector2[] _points;
		private static City[] _cities;
		private static List<Country> _countries;
		//private static Connection<City>[] _highways;
		private static QFont _font;
		private static VertexBuffer _buffer;

		static void Main() {
			_countries = Country.LoadCountries("countries.dat", "cities").Where(o => o.Cities.Count >= 2).ToList();
			_cities = _countries.SelectMany(o => o.Cities).ToArray();
			_points = _cities.Select(o => new Vector2((float)o.Latitude, (float)o.Longitude)).ToArray();

			var maxLat = _points.Max(o => o.Y);
			var maxLon = _points.Max(o => o.X);

			var minLat = _points.Min(o => o.Y);
			var minLon = _points.Min(o => o.X);

			var height = Math.Abs(maxLat - minLat);
			var scaleHeight = height / 2;
			var width = Math.Abs(maxLon - minLon);
			var scaleWidth = width / 2;
			
			minLat = _points.Min(o => o.Y) / scaleHeight;
			minLon = _points.Min(o => o.X) / scaleWidth;

			var addHeight = -minLat - 1;

			var addWidth = -minLon - 1;
			addWidth -= 0.2f;

			_points.ForEach(o => {
				var d = (o.X/scaleWidth) + addWidth;
				if (d < -1)
					d = 2 + d;
				o.X = d;
				o.Y = (o.Y/scaleHeight) + addHeight;
			});

			//_highways = _countries.SelectMany(o => o.Cities.SelectMany(p => p.BorderCities.Select(q => new Connection<City>(p, q)))).Distinct().ToArray();

			using (var game = new GameWindow(1280, 720, new GraphicsMode(32, 24, 0, 4))) {
				game.Load += (sender, e) => {
					game.VSync = VSyncMode.Off;
					GL.PointSize(2);
					GL.ClearColor(0f, 0f, 0f, 1f);
					_buffer = new VertexBuffer();
					_buffer.SetData(_points);

					_font = new QFont(".\\fonts\\pixelmix_micro.ttf", 12) {Options = {LockToPixel = true, Monospacing = QFontMonospacing.Yes}};
				};
				game.UpdateFrame += (sender, args) => {
					//_selectedCountry = _countries.RandomSubset(1).First();
					//_pathingCities = _selectedCountry?.Cities.RandomSubset(2).ToArray();
					//if (_pathingCities != null && _selectedCountry != null)
					//	_calculateShortestPathBetween = AStar.FindPath(_pathingCities[0], _pathingCities[1], Distance, Estimate);
				};

				game.Resize += (sender, e) => {
					GL.Viewport(0, 0, game.Width, game.Height);
					QFont.InvalidateViewport();
				};
				game.KeyDown += (o, eventArgs) => {
					if (eventArgs.Key == Key.Escape)
						game.Exit();
				};

				game.RenderFrame += (sender, e) => {
					// render graphics
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
					GL.PopAttrib();
					GL.MatrixMode(MatrixMode.Projection);
					GL.LoadIdentity();
					GL.Ortho(-1, 1, -1, 1, 0.0, 4.0);

					

					//GL.Begin(PrimitiveType.Lines);
					//GL.LineWidth(0.5f);

					//for (var i = 0; i < _highways.Length; i++) {
					//	var d = (_highways[i].LocationA.Longitude / scaleWidth) + addWidth;
					//	if (d < -1)
					//		d = 2 + d;
					//	var d1 = (_highways[i].LocationB.Longitude / scaleWidth) + addWidth;
					//	if (d1 < -1)
					//		d1 = 2 + d1;

					//	if (_highways[i].LocationA.IsHull && _highways[i].LocationB.IsHull) GL.Color3(0.0f, 0.2f, 0.0f);
					//	else GL.Color3(0.2f, 0.2f, 0.2f);
					//	GL.Vertex2(d, (_highways[i].LocationA.Latitude / scaleHeight) + addHeight);
					//	GL.Vertex2(d1, (_highways[i].LocationB.Latitude / scaleHeight) + addHeight);
					//}
					//GL.End();
					GL.Color3(1f,1f,1f);
					_buffer.Render();
					//GL.Begin(PrimitiveType.Points);
					//for (var i = 0; i < _points.Length; i++) {
					//	if (_cities[i].Country.Outbound.ContainsValue(_cities[i])) {
					//		GL.End();
					//		GL.Begin(PrimitiveType.Lines);

					//		var i1 = i;
					//		var outbound = _cities[i].Country.Outbound.Where(o => o.Value == _cities[i1]);
					//		GL.Color3(0.5f, 0.5f, 0f);
					//		outbound.ForEach(o => {
					//			var c = o.Key.Outbound[_cities[i1].Country];

					//			var d = (c.Longitude / scaleWidth) + addWidth;
					//			if (d < -1)
					//				d = 2 + d;
					//			var d1 = (_cities[i1].Longitude / scaleWidth) + addWidth;
					//			if (d1 < -1)
					//				d1 = 2 + d1;
					//			GL.Vertex2(d, (c.Latitude / scaleHeight) + addHeight);
					//			GL.Vertex2(d1, (_cities[i1].Latitude / scaleHeight) + addHeight);
					//		});

					//		GL.End();
					//		GL.Begin(PrimitiveType.Points);
					//		GL.Color3(0.5f, 0.5f, 0f);
					//	} else if (_cities[i].IsHull)
					//		GL.Color3(0.0f, 0.5f, 0.0f);
					//	else
					//		GL.Color3(0.3f, 0.3f, 0.3f);
					//	GL.Vertex2(_points[i]);
					//}
					//GL.End();


					//GL.Begin(PrimitiveType.LineStrip);
					//GL.LineWidth(2f);
					//if (_calculateShortestPathBetween != null)
					//	foreach (var connection in _calculateShortestPathBetween) {
					//		var d = (connection.Longitude / scaleWidth) + addWidth;
					//		if (d < -1)
					//			d = 2 + d;

					//		GL.Color3(1f, 0f, 0f);
					//		GL.Vertex2(d, (connection.Latitude / scaleHeight) + addHeight);
					//	}
					//GL.End();

					//QFont.Begin();
					//_font.Print($"TPS: {Math.Ceiling(game.UpdateFrequency):000} ({game.UpdateTime*1000*1000:000.0}µs/tick), FPS: {Math.Ceiling(game.RenderFrequency):000} ({game.RenderTime*1000:00.00}ms/frame)");
					//QFont.End();
					//GL.Disable(EnableCap.Texture2D);
					//GL.Disable(EnableCap.Blend);
					
					game.SwapBuffers();
				};

				// Run the game at 60 updates per second
				game.Run(10, 60);
			}
		}

		private static double Estimate(City city) {
			if (!city.Distances.ContainsKey(_pathingCities[1]))
				city.Distances.Add(_pathingCities[1], Connection<City>.Distance(city, _pathingCities[1]));

			return city.Distances[_pathingCities[1]] * 1.1;
		}

		private static double Distance(City city, City city1) {
			if (!city.Distances.ContainsKey(city1))
				city.Distances.Add(city1, Connection<City>.Distance(city, city1));

			return city.Distances[city1];
		}
	}
}

