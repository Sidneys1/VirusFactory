using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using QuickFont;
using VirusFactory.OpenTK.GameHelpers;

namespace VirusFactory.OpenTK.FSM.Elements {
    public class FloatTextElement : TextElement {

        #region Fields

        public new Vector2 Position {
            get { return _position; }
            set {
                _position = value;
                if (MousePosition == default(Vector2))
                    base.Position = _position;
            }
        }

        private Color4 _color;
        private Vector2 _position;

        public new Color4 Color {
            get { return _color; }
            set {
                _color = value;
                if (!MouseOverColor.HasValue || !IsMouseOver)
                    base.Color = _color;
            }
        }

        public Color4? MouseOverColor { get; set; } = null;

        #endregion Fields

        #region Properties

        public float Floatiness { get; set; } = 25f;

        #endregion Properties

        #region Constructors

        public FloatTextElement(GameWindow owner, string text, string fontFile, float size, FontStyle style = FontStyle.Regular) : base(owner, text, fontFile, size, style) {
            Color = base.Color;
        }

        public FloatTextElement(GameWindow owner, string text, Font font) : base(owner, text, font) {
            Color = base.Color;
        }

        public FloatTextElement(GameWindow owner, string text, QFont font) : base(owner, text, font) {
            Color = base.Color;
        }

        #endregion Constructors

        #region Methods

        public override void MouseMove(MouseMoveEventArgs e) {
            base.MouseMove(e);
            var floatPos = EaseMouse(MousePosition);
            base.Position = Position + (floatPos / (Floatiness * 2));
            
            if (MouseOverColor.HasValue) {
                
                base.Color = IsMouseOver ? MouseOverColor.Value : Color;
            }
        }

        public override void MouseWheel(MouseWheelEventArgs e) {
            // Do Nothing
        }

        private static Vector2 EaseMouse(Vector2 t) {
            return new Vector2(EaseMouse(t.X), EaseMouse(t.Y));
        }

        private static float EaseMouse(float t) {
            if (t < 0)
                return -Easing.EaseOut(-t, EasingType.Quadratic);
            return Easing.EaseOut(t, EasingType.Quadratic);
        }

        #endregion Methods
    }
}
