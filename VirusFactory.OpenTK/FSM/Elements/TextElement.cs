using OpenTK;
using OpenTK.Graphics;
using QuickFont;
using System.Drawing;

namespace VirusFactory.OpenTK.FSM.Elements {

    public class TextElement : UiElement {

        #region Fields

        private readonly Font _innerFont;
        private readonly string _fontFile;
        private readonly float _ptSize;
        private readonly FontStyle _style;

        private ProcessedText _processedText;
        private TransformViewport _viewport = new TransformViewport(-1, -1, 2, 2);
        private Color4 _color = Color4.White;
        private QFontAlignment _alignment = QFontAlignment.Centre;
        private float _maxWidth = float.MaxValue;
        private SizeF _size = SizeF.Empty;
        private RectangleF _bounds = RectangleF.Empty;
        private string _text;

        #endregion Fields

        #region Properties

        public QFont Font { get; private set; }

        public string Text {
            get { return _text; }
            set {
                _text = value;
                if (Font != null)
                    _processedText = Font.ProcessText(Text, MaxWidth, Alignment);
            }
        }

        public bool Dynamic { get; set; }

        public float MaxWidth {
            get { return _maxWidth; }
            set {
                _maxWidth = value;
                if (!Dynamic && Font != null)
                    _processedText = Font.ProcessText(Text, MaxWidth, Alignment);
            }
        }

        public QFontAlignment Alignment {
            get { return _alignment; }
            set {
                _alignment = value;
                if (!Dynamic && Font != null)
                    _processedText = Font.ProcessText(Text, MaxWidth, Alignment);
            }
        }

        public override Vector2 Position { get; set; } = Vector2.Zero;

        public TransformViewport Viewport {
            get { return _viewport; }
            set {
                _viewport = value;
                if (Font != null)
                    Font.Options.TransformToViewport = _viewport;
            }
        }

        public override SizeF Size {
            get {
                if (Dynamic)
                    return Font.Measure(Text, MaxWidth, Alignment);

                if (_size == SizeF.Empty)
                    _size = Font.Measure(_processedText);

                return _size;
            }
        }

        public override RectangleF Bounds => new RectangleF(new PointF(Position.X - (Size.Width / 2), Position.Y), Size);

        #endregion Properties

        #region Constructors

        public TextElement(GameWindow owner, string text, string fontFile, float size, FontStyle style = FontStyle.Regular) : base(owner) {
            Text = text;
            _fontFile = fontFile;
            _ptSize = size;
            _style = style;
        }

        public TextElement(GameWindow owner, string text, Font font) : base(owner) {
            Text = text;
            _innerFont = font;
        }

        public TextElement(GameWindow owner, string text, QFont font) : base(owner) {
            Text = text;
            Font = font;
        }

        #endregion Constructors

        #region Methods

        public override void Load() {
            if (Font == null)
                Font = _fontFile != null ? new QFont(_fontFile, _ptSize, _style) : new QFont(_innerFont);
            Font.Options.TransformToViewport = Viewport;
            Font.Options.Colour = Color;

            if (!Dynamic)
                _processedText = Font.ProcessText(Text, MaxWidth, Alignment);
        }

        public override void RenderFrame(FrameEventArgs e) {
            base.RenderFrame(e);
            Font.Options.Colour = Color;
            QFont.Begin();
            if (Dynamic)
                Font.Print(Text, Alignment, Position + PositionAdd);
            else
                Font.Print(_processedText, Position + PositionAdd);
            QFont.End();
        }

        public override Color4? MouseOverColor { get; set; }

        public override string ToString() {
            return Text;
        }

        public override void Resize() {
            base.Resize();
            QFont.InvalidateViewport();
        }

        #endregion Methods
    }
}