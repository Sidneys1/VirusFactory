using System;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using VirusFactory.OpenTK.FSM.Interface;

namespace VirusFactory.OpenTK.FSM.Elements {
    public abstract class UiElement : GameElementBase, IRenderable, IInputtable {
        protected Vector2 MousePosition;

        protected UiElement(GameWindow owner) : base(owner) {
        }

        public abstract SizeF Size { get; }
        public abstract RectangleF Bounds { get; }
        public bool IsMouseOver { get; private set; }
        public bool IsMouseDown { get; private set; }
        public event Action<EventArgs> Clicked;

        public virtual void RenderFrame(FrameEventArgs e) { }
        public virtual void KeyDown(KeyboardKeyEventArgs e) { }
        public virtual void KeyPress(KeyPressEventArgs e) { }
        public virtual void KeyUp(KeyPressEventArgs e) { }

        public virtual void MouseDown(MouseButtonEventArgs e) {
            if (IsMouseOver && e.Button == MouseButton.Left)
                IsMouseDown = true;
        }

        public virtual void MouseUp(MouseButtonEventArgs e) {
            if (!IsMouseDown) return;
            IsMouseDown = false;

            if (IsMouseOver)
                Clicked?.Invoke(new EventArgs());
        }

        public virtual void MouseEnter() { }
        public virtual void MouseLeave() { }

        public virtual void MouseMove(MouseMoveEventArgs e) {
            MousePosition = new Vector2(e.X / (float)Owner.Width, e.Y / (float)Owner.Height) * 2 - new Vector2(1f, 1f);
            IsMouseOver = Bounds.Contains(MousePosition.X, MousePosition.Y);
        }

        public virtual void MouseWheel(MouseWheelEventArgs e) { }
    }
}