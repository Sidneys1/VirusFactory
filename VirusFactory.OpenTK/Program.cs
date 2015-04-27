using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using VirusFactory.Model.Algorithm;
using VirusFactory.Model.Geography;
using VirusFactory.OpenTK.VBOHelper;

// ReSharper disable PossibleUnintendedReferenceComparison

// ReSharper disable AccessToDisposedClosure

namespace VirusFactory.OpenTK {
	class Program {
		private static Country _selectedCountry;
		private static City[] _pathingCities;
		private static Path<City> _path;
		private static BufferElement[] _points, _highways;
		private static City[] _cities;
		private static List<Country> _countries;
		private static QFont _debugFont, _uiFont;
		private static VertexBuffer _citiesBuffer, _highwayBuffer;
		private static double _elapsed;

		private static long _tick;

		static void Main() {
			_countries = Country.LoadCountries("countries.dat", "cities").Where(o => o.Cities.Count >= 2).ToList();
			_cities = _countries.SelectMany(o => o.Cities).ToArray();
			_points = _cities.Select(o => new BufferElement(new Vector2((float)o.Point.X, (float)o.Point.Y),
				o.IsHull
				? (o.Country.Outbound.ContainsValue(o)
					? new Vector4(1f, 1f, 0f,1f)
					: new Vector4(0f, 1f, 0f,1f))
				: new Vector4(0.3f, 0.3f, 0.3f, 1f))).ToArray();

			float scaleHeight,scaleWidth,addHeight,addWidth;
			Scaling(out scaleHeight, out scaleWidth, out addHeight, out addWidth);

			TransformPoints(scaleWidth, addWidth, scaleHeight, addHeight);

			_highways = GenHighways(scaleWidth, addWidth, scaleHeight, addHeight);

			_selectedCountry = _countries.First(o=>o.Name == "United States");

			using (var game = new GameWindow(1280, 720, new GraphicsMode(32, 24, 0, 8))) {
				game.Load += (sender, e) => {
					game.VSync = VSyncMode.On;
					GL.PointSize(3);
					GL.ClearColor(0f, 0f, 0f, 1f);
					

					_citiesBuffer = new VertexBuffer(_points);
					_highwayBuffer = new VertexBuffer(_highways);

					_debugFont = new QFont(".\\fonts\\pixelmix_micro.ttf", 6) { Options = { LockToPixel = true, Monospacing = QFontMonospacing.Yes } };
					_uiFont = new QFont(".\\fonts\\pixelmix.ttf", 12) { Options = { LockToPixel = true } };
				};
				game.UpdateFrame += (sender, args) => {
					if (_tick%15 == 0) {
						//_selectedCountry = _countries.RandomSubset(1).First();
						_pathingCities = _selectedCountry?.Cities.RandomSubset(2).ToArray();
						if (_pathingCities != null && _selectedCountry != null)
							_path = AStar.FindPath(_pathingCities[0], _pathingCities[1], Distance, Estimate);
					}
					GL.BindBuffer(BufferTarget.ArrayBuffer, _citiesBuffer.Id);
					var videoMemoryIntPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadWrite);
					unsafe
					{
						var videoMemory = (BufferElement*)videoMemoryIntPtr.ToPointer();
						for (var i = 0; i < _points.Length; i++) {
							var sin = (float)Math.Sin(_elapsed);
                            videoMemory[i].Color = new Vector4(videoMemory[i].Color.Xyz, sin);
						}
					}
					GL.UnmapBuffer(BufferTarget.ArrayBuffer);
					_elapsed += args.Time;
					_tick++;
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
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
					GL.PopAttrib();
					GL.MatrixMode(MatrixMode.Projection);
					GL.LoadIdentity();
					GL.Ortho(-1, 1, -1, 1, 0.0, 4.0);

					GL.Enable(EnableCap.PointSmooth);
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

					_highwayBuffer.Render(PrimitiveType.Lines);
					_citiesBuffer.Render();
					
					GL.Begin(PrimitiveType.LineStrip);
					GL.LineWidth(2f);
					if (_path != null)
						foreach (var connection in _path) {
							var d = (connection.Longitude / scaleWidth) + addWidth;
							if (d < -1)
								d = 2 + d;

							GL.Color3(1f, 0f, 0f);
							GL.Vertex2(d, (connection.Latitude / scaleHeight) + addHeight);
						}
					GL.End();

					QFont.Begin();
					_debugFont.Print($"TPS: {Math.Ceiling(game.UpdateFrequency):000} ({game.UpdateTime * 1000:0.000}ms/tick), FPS: {Math.Ceiling(game.RenderFrequency):000} ({game.RenderTime * 1000:0.000}ms/frame)");
					_uiFont.Print($"{_path?.FirstStep} -> {_path?.LastStep}", QFontAlignment.Centre, new Vector2(game.Width/2f,game.Height-20));
					QFont.End();
					GL.Disable(EnableCap.Texture2D);
					GL.Disable(EnableCap.Blend);

					game.SwapBuffers();
				};

				// Run the game at 60 updates per second
				game.Run(30, 60);
			}
		}

		private static void Scaling(out float scaleHeight, out float scaleWidth, out float addHeight, out float addWidth) {
			float maxLat;
			float maxLon;
			float minLat;
			float minLon;
			maxLat = _points.Max(o => o.Vertex.Y);
			maxLon = _points.Max(o => o.Vertex.X);

			minLat = _points.Min(o => o.Vertex.Y);
			minLon = _points.Min(o => o.Vertex.X);


			scaleHeight = Math.Abs(maxLat - minLat)/2;
			scaleWidth = Math.Abs(maxLon - minLon)/2;

			minLat = _points.Min(o => o.Vertex.Y)/scaleHeight;
			minLon = _points.Min(o => o.Vertex.X)/scaleWidth;

			addHeight = -minLat - 1;

			addWidth = -minLon - 1;
			addWidth -= 0.2f;
		}

		private static void TransformPoints(float scaleWidth, float addWidth, float scaleHeight, float addHeight) {
			for (var i = 0; i < _points.Length; i++) {
				var o = _points[i];
				var d = (o.Vertex.X/scaleWidth) + addWidth;
				if (d < -1)
					d = 2 + d;
				_points[i] = new BufferElement(new Vector2(d, (o.Vertex.Y/scaleHeight) + addHeight), o.Color);
			}
		}

		private static BufferElement[] GenHighways(float scaleWidth, float addWidth, float scaleHeight, float addHeight) {
			return _countries.SelectMany(o => o.Cities.SelectMany(p => p.BorderCities.Select(q => new Connection<City>(p, q)))).Distinct()
				.SelectMany(o => {
					var x1 = (o.LocationA.Point.X / scaleWidth) + addWidth;
					var y1 = (o.LocationA.Point.Y / scaleHeight) + addHeight;
					var x2 = (o.LocationB.Point.X/scaleWidth) + addWidth;
					var y2 = (o.LocationB.Point.Y / scaleHeight) + addHeight;
					if (x1 < -1)
						x1 = 2 + x1;
					if (x2 < -1)
						x2 = 2 + x2;
					return new[] {
						new BufferElement(new Vector2((float)x1,(float)y1), o.LocationA.IsHull ? new Vector4(0f, 0.4f, 0f,1f) : new Vector4(0.2f, 0.2f, 0.2f,1f)),
						new BufferElement(new Vector2((float)x2,(float)y2), o.LocationB.IsHull ? new Vector4(0f, 0.4f, 0f,1f) : new Vector4(0.2f, 0.2f, 0.2f,1f))
					};
				}).ToArray();
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

