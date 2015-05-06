using MoreLinq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VirusFactory.Model.Geography;
using VirusFactory.OpenTK.FSM.Elements;
using VirusFactory.OpenTK.FSM.Interface;
using VirusFactory.OpenTK.GameHelpers;
using VirusFactory.OpenTK.GameHelpers.VBOHelper;

namespace VirusFactory.OpenTK.FSM.States {
    public class IngameState : GameStateBase, IResizable, IKeyboardInput {
        #region Fields

        private readonly TextElement _debugText;
        private List<Country> _countries;
        private City[] _cities;
        private BufferElement[] _points;
        private float _scale;
        private Vector2 _add;
        private RectangleF _bounds;
        private BufferElement[] _highways;
        private Bounds _viewPort;

        #endregion Fields

        #region Properties

        public World World { get; set; }

        #endregion Properties

        #region Ctor / Dtor

        public IngameState(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
            _debugText = new TextElement(owner, "debug me", ".\\fonts\\pixelmix_micro.ttf", 6f) {
                Alignment = QFontAlignment.Left,
                Dynamic = true,
                Position = new Vector2(-1f, -1f)
            };
        }

        #endregion Ctor / Dtor

        #region Methods

        public override void Load() {
            World = World.Load("world.dat");
            _countries = World.Countries.Where(o => o.Cities.Count >= 2).ToList();
            _cities = _countries.SelectMany(o => o.Cities).ToArray();
            _points = _cities.Select(GenBufferElement).ToArray();

            Scaling(out _scale, out _add, out _bounds);

            TransformPoints(_scale, _add);

            _highways = GenHighways(_scale, _add);

            Action prep = () =>
            {
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.VertexPointer(2, VertexPointerType.Float, BufferElement.SizeInBytes, new IntPtr(0));
                GL.ColorPointer(4, ColorPointerType.Float, BufferElement.SizeInBytes, new IntPtr(Vector2.SizeInBytes));
            };
            GameElements.Add(new VboElement<BufferElement>(Owner, new VertexBuffer<BufferElement>(_highways, prep), PrimitiveType.Lines));
            GameElements.Add(new VboElement<BufferElement>(Owner, new VertexBuffer<BufferElement>(_points, prep), PrimitiveType.Points));
            GameElements.Add(_debugText);
            base.Load();
            _debugText.Font.Options.Monospacing = QFontMonospacing.Yes;
            SetViewport();
        }

        public override void UpdateFrame(FrameEventArgs e) {
            base.UpdateFrame(e);

            _debugText.Text =
                $"TPS: {Math.Ceiling(Owner.UpdateFrequency):0000} ({Owner.UpdateTime * 1000:0.000}ms/tick), FPS: {Math.Ceiling(Owner.RenderFrequency):0000} ({Owner.RenderTime * 1000:00.000}ms/frame)";
        }

        public void Resize() {
            GL.Viewport(0, 0, Owner.Width, Owner.Height);
            QFont.InvalidateViewport();
            SetViewport();
        }

        public override void RenderFrame(FrameEventArgs e) {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.PopAttrib();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(_viewPort.Left, _viewPort.Right, _viewPort.Bottom, _viewPort.Top, 0.0, 4.0);

            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            base.RenderFrame(e);

            Owner.SwapBuffers();
        }

        private BufferElement GenBufferElement(City o) {
            var c = GdpStretch(o.Country);

            var color = new Vector4(0.4f, 0.4f, 0.4f, c);

            if (ReferenceEquals(o.Country.Cities[0], o))
                color = new Vector4(0f, 0f, 1f, c);
            else if (o.IsHull) {
                color = o.Country.Outbound.ContainsValue(o) ? new Vector4(1f, 1f, 0f, c) : new Vector4(0f, 1f, 0f, c);
            }

            return new BufferElement(new Vector2((float)o.Point.X, (float)o.Point.Y), color);
        }

        private float GdpStretch(Country o) {
            var t = (float)o.GdpPerCapita;
            var d = (float)World.Countries.Max(y => y.GdpPerCapita);
            t /= d;
            return -0.9f * t * (t - 2) + 0.1f;
        }

        private void Scaling(out float scale, out Vector2 add, out RectangleF bounds) {
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

        private void TransformPoints(float scale, Vector2 add) {
            for (var i = 0; i < _points.Length; i++) {
                var f = (_points[i].Vertex.X / scale) + add.X;
                if (f < -1)
                    f += 2;
                _points[i] = new BufferElement(new Vector2(f, (_points[i].Vertex.Y / scale) + add.Y), _points[i].Color);
            }
        }

        private BufferElement[] GenHighways(float scale, Vector2 add) {
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

        private void SetViewport() {
            var fatness1 = Owner.Width / (float)Owner.Height;
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

        public void KeyDown(KeyboardKeyEventArgs e) {
            GameElements.OfType<IKeyboardInput>().ForEach(o => o.KeyDown(e));
        }

        public void KeyPress(KeyPressEventArgs e) {
            GameElements.OfType<IKeyboardInput>().ForEach(o => o.KeyPress(e));
        }

        public void KeyUp(KeyboardKeyEventArgs e) {
            GameElements.OfType<IKeyboardInput>().ForEach(o => o.KeyUp(e));

            if (e.Key == Key.Escape)
                StateMachine.Transition("pause");
        }

        #endregion Methods
    }
}