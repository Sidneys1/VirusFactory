using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using Behaviorals;
using VirusFactory.OpenTK.FSM.Behaviours;
using VirusFactory.OpenTK.FSM.Interface;

namespace VirusFactory.OpenTK.FSM.Elements {

    public abstract class UiElement : GameElementBase, IBehavioral<UiElement>, IRenderable, IKeyboardInput, IMouseInput, IResizable {

        #region Properties

        public Vector2 MousePosition { get; protected set; }

        public abstract SizeF Size { get; }

        public abstract RectangleF Bounds { get; }

        public bool IsMouseOver { get; private set; }

        public bool IsMouseDown { get; private set; }

        public virtual Vector2 Position { get; set; }

        public virtual Vector2 PositionAdd { get; set; }

        public virtual Color4 NormalColor { get; set; } = Color4.White;

        public virtual Color4 Color => MouseOverColor.HasValue && IsMouseOver ? MouseOverColor.Value : NormalColor;

        public MultiMap<int, Behaviour<UiElement>> Behaviours { get; } = new MultiMap<int, Behaviour<UiElement>>();

        public Dictionary<string, object> AttachedProperties { get; } = new Dictionary<string, object>();

        public abstract Color4? MouseOverColor { get; set; }

        #endregion Properties

        #region Events

        public event Action<EventArgs> Clicked;

        #endregion Events

        #region Constructors

        protected UiElement(GameWindow owner) : base(owner) {
        }

        #endregion Constructors

        #region Methods

        public virtual void RenderFrame(FrameEventArgs e) {
        }

        public virtual void KeyDown(KeyboardKeyEventArgs e) {
        }

        public virtual void KeyPress(KeyPressEventArgs e) {
        }

        public virtual void KeyUp(KeyboardKeyEventArgs e) {
        }

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

        public virtual void MouseEnter() {
        }

        public virtual void MouseLeave() {
        }

        public virtual void MouseMove(MouseMoveEventArgs e) {
            MousePosition = new Vector2(e.X / (float)Owner.Width, e.Y / (float)Owner.Height) * 2 - new Vector2(1f, 1f);
            IsMouseOver = Bounds.Contains(MousePosition.X, MousePosition.Y);

            Trigger(GameTriggers.MouseMove);
        }

        public virtual void MouseWheel(MouseWheelEventArgs e) {
        }

        public void Trigger(int trigger) {
            if (!Behaviours.ContainsKey(trigger)) return;
            foreach (var behaviourBase in Behaviours[trigger]) {
                behaviourBase.Action.Invoke(this);
            }
        }

        #endregion Methods

        public virtual void Resize() {
            
        }
    }
}