using MoreLinq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using GFSM;
using VirusFactory.Model.Algorithm;
using VirusFactory.Model.Geography;
using VirusFactory.OpenTK.FSM;
using VirusFactory.OpenTK.FSM.States;
using VirusFactory.OpenTK.GameHelpers;
using VirusFactory.OpenTK.GameHelpers.VBOHelper;

// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable AccessToDisposedClosure

namespace VirusFactory.OpenTK {

    internal class Program {

        #region Fields

        public static World World;
        private static readonly TickScheduler Scheduler = new TickScheduler();
        private static List<Country> _countries;
        private static City[] _cities;
        private static Country[] _selectedCountry;
        private static Path<City> _path;
        private static BufferElement[] _points, _highways, _paths;
        private static VertexBuffer<BufferElement> _citiesBuffer, _highwayBuffer, _pathBuffer;
        private static QFont _debugFont, _uiFont;
        private static double _elapsed;
        private static float _scale;
        private static Vector2 _add;
        private static RectangleF _bounds;
        private static Bounds _viewPort;
        private static WindowState _previousState;

        #endregion Fields

        #region Methods

        private static void Main() {
            var fsm = new GameFiniteStateMachine();
            using (var game = new GameWindow(1280, 720, new GraphicsMode(32, 24, 0, 8))) {
                GameStateBase mainMenuState, ingameState, settingsMenuState, pauseMenuState;

                mainMenuState = new MainMenuState(game, fsm);
                ingameState = new IngameState(game, fsm);
                settingsMenuState = new SettingMenuState(game, fsm);
                pauseMenuState = new PauseMenuState(game, fsm);

                fsm.States.Add(mainMenuState);
                fsm.States.Add(ingameState);
                fsm.States.Add(settingsMenuState);
                fsm.States.Add(pauseMenuState);

                fsm.AddTransition(new Transition<GameStateBase>("init", null, mainMenuState));
                fsm.AddTransition(new Transition<GameStateBase>("start", mainMenuState, ingameState));
                fsm.AddTransition(new Transition<GameStateBase>("settings", mainMenuState, settingsMenuState));
                fsm.AddTransition(new Transition<GameStateBase>("settings", pauseMenuState, settingsMenuState));
                fsm.AddTransition(new Transition<GameStateBase>("pause", ingameState, pauseMenuState));
                fsm.AddTransition(new Transition<GameStateBase>("main menu", pauseMenuState, mainMenuState, Mode.Pop));

                fsm.AddTransition(new Transition<GameStateBase>("return", settingsMenuState, mainMenuState, Mode.Pop));
                fsm.AddTransition(new Transition<GameStateBase>("return", settingsMenuState, pauseMenuState, Mode.Pop));
                fsm.AddTransition(new Transition<GameStateBase>("return", pauseMenuState, ingameState, Mode.Pop));

                fsm.AddTransition(new Transition<GameStateBase>("exit", mainMenuState, null));
                fsm.AddTransition(new Transition<GameStateBase>("exit", pauseMenuState, null));

                fsm.Transitioned += t => { if (t.To == null) game.Exit(); };

                fsm.Transition("init");

                game.Load += (sender, args) => fsm.Load();
                game.Unload += (sender, args) => fsm.UnLoad();

                game.KeyDown += (sender, args) => fsm.KeyDown(args);
                game.KeyUp += (sender, args) => fsm.KeyUp(args);
                game.KeyPress += (sender, args) => fsm.KeyPress(args);
                
                game.MouseMove += (sender, args) => fsm.MouseMove(args);
                game.MouseDown += (sender, args) => fsm.MouseDown(args);
                game.MouseUp += (sender, args) => fsm.MouseUp(args);
                game.MouseEnter += (sender, args) => fsm.MouseEnter();
                game.MouseLeave += (sender, args) => fsm.MouseLeave();
                game.MouseWheel += (sender, args) => fsm.MouseWheel(args);


                game.Resize += (sender, args) => fsm.Resize();

                game.RenderFrame += (sender, args) => fsm.RenderFrame(args);

                game.UpdateFrame += (sender, args) => fsm.UpdateFrame(args);

                game.Title = "Apoplexy";
                
                game.Icon = new Icon("Grim Reaper.ico");

                game.Run(30, 60);
            }

            return;
            var stop = new Stopwatch();

            stop.Start();
            {
                World = World.Load("world.dat");
            }
            stop.Stop();
            Console.WriteLine($"{stop.Elapsed}: Loading world.dat");

            _countries = World.Countries.Where(o => o.Cities.Count >= 2).ToList();
            _cities = _countries.SelectMany(o => o.Cities).ToArray();

            stop.Restart();
            {
                _points = _cities.Select(GenBufferElement).ToArray();
            }
            stop.Stop();
            Console.WriteLine($"{stop.Elapsed}: Generating buffer elements");

            Scaling(out _scale, out _add, out _bounds);

            stop.Restart();
            {
                TransformPoints(_scale, _add);
            }
            stop.Stop();
            Console.WriteLine($"{stop.Elapsed}: Transforming points");

            stop.Restart();
            {
                _highways = GenHighways(_scale, _add);
            }
            stop.Stop();
            Console.WriteLine($"{stop.Elapsed}: Gnerating Highways");
            //stop.Restart();

            using (var game = new GameWindow(1280, 720, new GraphicsMode(32, 24, 0, 8))) {
                game.Load += GameLoad;
                game.Closing += (sender, args) =>
                {
                    _citiesBuffer.Dispose();
                    _highwayBuffer.Dispose();
                    _pathBuffer.Dispose();
                    //game.KeyDown
                };
                game.UpdateFrame += GameUpdateFrame;
                game.RenderFrame += GameRenderFrame;
                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                    QFont.InvalidateViewport();
                    SetViewport(game);
                };
                game.KeyDown += (o, eventArgs) =>
                {
                    switch (eventArgs.Key) {
                        case Key.Escape:
                            game.Exit();
                            break;

                        case Key.F11:
                            if (game.WindowState == WindowState.Fullscreen)
                                game.WindowState = _previousState;
                            else {
                                _previousState = game.WindowState;
                                game.WindowState = WindowState.Fullscreen;
                            }
                            break;
                    }
                };
                game.Run(30, 60);
            }
        }

        private static void SetViewport(GameWindow game) {
            var fatness1 = game.Width / (float)game.Height;
            var fatness2 = _bounds.Width / _bounds.Height;

            double l, r, t, b;

            if (fatness2 >= fatness1) {
                l = _bounds.Left;
                r = _bounds.Right;
                var halfHeight = (r - l) / fatness1;
                halfHeight /= 2;
                t = halfHeight;
                b = -t;
            } else {
                b = _bounds.Top;
                t = _bounds.Bottom;
                var halfWidth = (t - b) * fatness1;
                halfWidth /= 2;
                r = halfWidth;
                l = -r;
            }

            _viewPort = new Bounds { Bottom = b, Left = l, Right = r, Top = t };
        }

        private static void GameRenderFrame(object sender, FrameEventArgs e) {
            var game = (GameWindow)sender;
            // render graphics
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.PopAttrib();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(_viewPort.Left, _viewPort.Right, _viewPort.Bottom, _viewPort.Top, 0.0, 4.0);

            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            _highwayBuffer.Render(PrimitiveType.Lines);
            _citiesBuffer.Render();
            _pathBuffer?.Render(PrimitiveType.Lines);
            QFont.Begin();
            _debugFont.Print(
                $"TPS: {Math.Ceiling(game.UpdateFrequency):0000} ({game.UpdateTime * 1000:0.000}ms/tick), FPS: {Math.Ceiling(game.RenderFrequency):0000} ({game.RenderTime * 1000:00.000}ms/frame)");
            _uiFont.Print($"{_path}", QFontAlignment.Centre, new Vector2(game.Width / 2f, game.Height - 20));
            QFont.End();

            game.SwapBuffers();
        }

        private static void GameUpdateFrame(object sender, FrameEventArgs args) {
            Scheduler.Tick((GameWindow)sender, args);
            _elapsed += args.Time;
        }

        private static void UpdatePath(TickItem sender, GameWindow game, FrameEventArgs e) {
            _selectedCountry = _countries.RandomSubset(2).ToArray();

            var pathingCities = new[] {
                _selectedCountry?[0].Cities.RandomSubset(1).First(),
                _selectedCountry?[1].Cities.RandomSubset(1).First()
            };

            var connected =
                _selectedCountry[0].BorderCountries.Any(
                    o => o == _selectedCountry[1] || o.BorderCountries.Contains(_selectedCountry[1]));
            if (connected) {
                var middleC = _selectedCountry[0].BorderCountries.Contains(_selectedCountry[1])
                    ? null
                    : _selectedCountry[0].BorderCountries.FirstOrDefault(
                        o => o.BorderCountries.Contains(_selectedCountry[1]));

                if (_selectedCountry == null) return;

                if (middleC != null) {
                    var path = new[] {
                        AStar.FindPath(pathingCities[0], _selectedCountry[0].Outbound[middleC], Distance, Distance),
                        AStar.FindPath(middleC.Outbound[_selectedCountry[0]], middleC.Outbound[_selectedCountry[1]],
                            Distance, Distance),
                        AStar.FindPath(_selectedCountry[1].Outbound[middleC], pathingCities[1], Distance, Distance)
                    };

                    path[2].FirstPath.PreviousSteps = path[1];
                    path[1].FirstPath.PreviousSteps = path[0];
                    pathingCities = path[2].ToArray();
                    _path = path[2];
                } else {
                    var path = new[] {
                        AStar.FindPath(pathingCities[0], _selectedCountry[0].Outbound[_selectedCountry[1]], Distance,
                            Distance),
                        AStar.FindPath(_selectedCountry[1].Outbound[_selectedCountry[0]], pathingCities[1], Distance,
                            Distance)
                    };

                    path[1].FirstPath.PreviousSteps = path[0];
                    pathingCities = path[1].ToArray();
                    _path = path[1];
                }
            } else {
                var path = new[] {
                    AStar.FindPath(pathingCities[0], _selectedCountry[0].Cities[0], Distance, Distance),
                    new Path<City>(_selectedCountry[1].Cities[0]) {
                        PreviousSteps = new Path<City>(_selectedCountry[0].Cities[0])
                    },
                    AStar.FindPath(_selectedCountry[1].Cities[0], pathingCities[1], Distance, Distance)
                };
                path[2].FirstPath.PreviousSteps = path[1];
                path[1].FirstPath.PreviousSteps = path[0];
                pathingCities = path[2].ToArray();
                _path = path[2];
            }

            _paths = new BufferElement[(pathingCities.Length - 1) * 2];

            for (var i = 0; i < pathingCities.Length - 1; i++) {
                var d = (float)((pathingCities[i].Longitude / _scale) + _add.X);
                if (d < -1)
                    d += 2;
                var d2 = (float)((pathingCities[i + 1].Longitude / _scale) + _add.X);
                if (d2 < -1)
                    d2 += 2;
                var color = pathingCities[i].Country == pathingCities[i + 1].Country ||
                            pathingCities[i].Country.BorderCountries.Contains(pathingCities[i + 1].Country)
                    ? new Vector4(1f, 0f, 0f, 1f)
                    : new Vector4(0f, 0f, 1f, 1f);

                _paths[i * 2] = new BufferElement(new Vector2(d, (float)((pathingCities[i].Latitude / _scale) + _add.Y)),
                    color);
                _paths[(i * 2) + 1] =
                    new BufferElement(new Vector2(d2, (float)((pathingCities[i + 1].Latitude / _scale) + _add.Y)), color);
            }

            _pathBuffer?.Dispose();
            _pathBuffer = new VertexBuffer<BufferElement>(_paths, () =>
            {
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.VertexPointer(2, VertexPointerType.Float, BufferElement.SizeInBytes, new IntPtr(0));
                GL.ColorPointer(4, ColorPointerType.Float, BufferElement.SizeInBytes, new IntPtr(Vector2.SizeInBytes));
            });
        }

        private static unsafe void UpdateVbos(TickItem sender, GameWindow game, FrameEventArgs e) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _citiesBuffer.Id);
            var videoMemoryIntPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadWrite);
            var videoMemory = (BufferElement*)videoMemoryIntPtr.ToPointer();
            for (var i = 0; i < _points.Length; i++) {
                var sin = (float)(Math.Sin((_elapsed / (Math.PI / 5)) + (videoMemory[i].Vertex.X * Math.PI)) / 2) + 0.5f;
                videoMemory[i].Color = new Vector4(videoMemory[i].Color.X, videoMemory[i].Color.Y,
                    videoMemory[i].Color.Z, sin * videoMemory[i].OriginalW);
            }
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
        }

        private static void GameLoad(object sender, EventArgs e) {
            ((GameWindow)sender).VSync = VSyncMode.Adaptive;
            GL.PointSize(2);
            GL.ClearColor(0f, 0f, 0f, 1f);

            Action prep = () =>
            {
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.VertexPointer(2, VertexPointerType.Float, BufferElement.SizeInBytes, new IntPtr(0));
                GL.ColorPointer(4, ColorPointerType.Float, BufferElement.SizeInBytes, new IntPtr(Vector2.SizeInBytes));
            };
            _citiesBuffer = new VertexBuffer<BufferElement>(_points, prep);
            _highwayBuffer = new VertexBuffer<BufferElement>(_highways, prep);

            _debugFont = new QFont(".\\fonts\\pixelmix_micro.ttf", 6) {
                Options = { LockToPixel = true, Monospacing = QFontMonospacing.Yes }
            };
            _uiFont = new QFont(".\\fonts\\pixelmix.ttf", 9) { Options = { LockToPixel = true } };

            // Scheduler setup
            Scheduler.TickItems.Add(new TickItem(UpdatePath, new TimeSpan(0, 0, 0, 0, 250), true));
            Scheduler.TickItems.Add(new TickItem(UpdateVbos, new TimeSpan(0, 0, 0, 0, 1000 / 30)));

            SetViewport((GameWindow)sender);
        }

        private static BufferElement GenBufferElement(City o) {
            var c = GdpStretch(o.Country);

            var color = new Vector4(0.4f, 0.4f, 0.4f, c);

            if (o.Country.Cities[0] == o)
                color = new Vector4(0f, 0f, 1f, c);
            else if (o.IsHull) {
                color = o.Country.Outbound.ContainsValue(o) ? new Vector4(1f, 1f, 0f, c) : new Vector4(0f, 1f, 0f, c);
            }

            return new BufferElement(new Vector2((float)o.Point.X, (float)o.Point.Y), color);
        }

        private static void Scaling(out float scale, out Vector2 add, out RectangleF bounds) {
            var maxY = _points.Max(o => o.Vertex.Y);
            var maxX = _points.Max(o => o.Vertex.X);

            var minY = _points.Min(o => o.Vertex.Y);
            var minX = _points.Min(o => o.Vertex.X);

            var scaleHeight = Math.Abs(maxY - minY) / 2;
            var scaleWidth = Math.Abs(maxX - minX) / 2;

            scale = Math.Max(scaleHeight, scaleWidth);

            add = new Vector2(-0.3f, 0);
            var addt = add;
            var scalet = scale;
            Func<float, float, float, float> selector = (o, s, a) =>
            {
                var f = (o / s) + a;
                if (f < -1)
                    f += 2;
                return f;
            };
            maxX = _points.Max(o => selector(o.Vertex.X, scalet, addt.X));
            maxY = _points.Max(o => selector(o.Vertex.Y, scalet, addt.Y));
            minX = _points.Min(o => selector(o.Vertex.X, scalet, addt.X));
            minY = _points.Min(o => selector(o.Vertex.Y, scalet, addt.Y));

            add.Y = -(minY + ((maxY - minY) / 2));
            bounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        private static void TransformPoints(float scale, Vector2 add) {
            for (var i = 0; i < _points.Length; i++) {
                var f = (_points[i].Vertex.X / scale) + add.X;
                if (f < -1)
                    f += 2;
                _points[i] = new BufferElement(new Vector2(f, (_points[i].Vertex.Y / scale) + add.Y), _points[i].Color);
            }
        }

        private static BufferElement[] GenHighways(float scale, Vector2 add) {
            return _countries.SelectMany(o =>
            {
                return
                    o.Cities.SelectMany(p => p.BorderCities.Select(q => new Connection<City>(p, q)))
                        .Distinct()
                        .SelectMany(x =>
                        {
                            var f = GdpStretch(o);
                            var x3 = (x.LocationA.Point.X / scale) + add.X;
                            var y3 = (x.LocationA.Point.Y / scale) + add.Y;
                            var x4 = (x.LocationB.Point.X / scale) + add.X;
                            var y4 = (x.LocationB.Point.Y / scale) + add.Y;
                            if (x3 < -1)
                                x3 += 2;
                            if (x4 < -1)
                                x4 += 2;
                            return new[] {
                                new BufferElement(new Vector2((float) x3, (float) y3),
                                    x.LocationA.IsHull ? new Vector4(0f, 0.4f, 0f, f) : new Vector4(0.2f, 0.2f, 0.2f, f)),
                                new BufferElement(new Vector2((float) x4, (float) y4),
                                    x.LocationB.IsHull ? new Vector4(0f, 0.4f, 0f, f) : new Vector4(0.2f, 0.2f, 0.2f, f))
                            };
                        });
            }).ToArray();
        }

        private static float GdpStretch(Country o) {
            var t = (float)o.GdpPerCapita;
            var d = (float)World.Countries.Max(y => y.GdpPerCapita);
            t /= d;
            return -0.9f * t * (t - 2) + 0.1f;
        }

        private static double Distance(City city, City city1) {
            if (!city.Distances.ContainsKey(city1))
                city.Distances.Add(city1, Connection<City>.Distance(city, city1));

            return city.Distances[city1];
        }

        #endregion Methods
    }
}