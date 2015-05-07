using MoreLinq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using System;
using System.Linq;
using Behaviorals;
using VirusFactory.OpenTK.FSM.Behaviours;
using VirusFactory.OpenTK.FSM.Elements;
using VirusFactory.OpenTK.FSM.Elements.Base;
using VirusFactory.OpenTK.FSM.Interface;
using VirusFactory.OpenTK.GameHelpers;

namespace VirusFactory.OpenTK.FSM.States.Base {
	public class MenuStateBase : GameStateBase, IKeyboardInput, IMouseInput {
		#region Protected Fields

		protected bool Transitioning;

		#endregion Protected Fields

		#region Private Fields

		private const double SECONDS = 0.5;
		protected const string FLOATINESS_PROP = "floatiness";
		private Vector2 _vportOffset;

		protected static Behaviour<UiElementBase> FloatBehavior = new Behaviour<UiElementBase>(o =>
		{
			var floatPos = BehaviorHelpers.EaseMouse(o.MousePosition);
			o.PositionAdd = floatPos / ((float)o.AttachedProperties[FLOATINESS_PROP]);
		});

		#endregion Private Fields

		#region Public Properties

		public Vector2 VportOffset {
			get { return _vportOffset; }
			private set {
				_vportOffset = value;
				GameElements.OfType<TextElement>().ForEach(t => t.Viewport = new TransformViewport(_vportOffset.X - 1, VportOffset.Y - 1, 2, 2));
			}
		}

		#endregion Public Properties

		#region Protected Ctor / Dtor

		protected MenuStateBase(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
		}

		#endregion Protected Ctor / Dtor

		#region Public Methods

		public override void RenderFrame(FrameEventArgs e) {
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
			GL.PopAttrib();
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(-1 + VportOffset.X, 1 + VportOffset.X, -1 + VportOffset.Y, 1 + VportOffset.Y, 0.0, 4.0);

			base.RenderFrame(e);

			Owner.SwapBuffers();
		}

		public virtual void KeyDown(KeyboardKeyEventArgs e) {
			GameElements.OfType<IKeyboardInput>().ForEach(o => o.KeyDown(e));

			if (e.Key == Key.Escape)
				TransitionOut("return");
		}

		public virtual void KeyPress(KeyPressEventArgs e) {
			GameElements.OfType<IKeyboardInput>().ForEach(o => o.KeyPress(e));
		}

		public virtual void KeyUp(KeyboardKeyEventArgs e) {
			GameElements.OfType<IKeyboardInput>().ForEach(o => o.KeyUp(e));
		}

		public virtual void MouseDown(MouseButtonEventArgs e) {
			GameElements.OfType<IMouseInput>().ForEach(o => o.MouseDown(e));
		}

		public virtual void MouseUp(MouseButtonEventArgs e) {
			GameElements.OfType<IMouseInput>().ForEach(o => o.MouseUp(e));
		}

		public virtual void MouseEnter() {
			GameElements.OfType<IMouseInput>().ForEach(o => o.MouseEnter());
		}

		public virtual void MouseLeave() {
			GameElements.OfType<IMouseInput>().ForEach(o => o.MouseLeave());
		}

		public virtual void MouseMove(MouseMoveEventArgs e) {
			GameElements.OfType<IMouseInput>().ForEach(o => o.MouseMove(e));
		}

		public virtual void MouseWheel(MouseWheelEventArgs e) {
			GameElements.OfType<IMouseInput>().ForEach(o => o.MouseWheel(e));
		}

		public override void Enter() {
			base.Enter();
			TransitionIn();
		}

		public override void Load() {
			base.Load();
			VportOffset = new Vector2(2, 0);
		}

		public override void Resize() {
			base.Resize();
			VportOffset = VportOffset;
		}

		#endregion Public Methods

		#region Protected Methods

		protected void TransitionOut(string token) {
			Transitioning = true;
			Scheduler.TickItems.Add(CreateOutTransition(token));
		}

		protected void TransitionIn() {
			Transitioning = true;
			Scheduler.TickItems.Add(CreateInTransition());
		}

		#endregion Protected Methods

		#region Private Methods

		private TickItem CreateInTransition() {
			var inTransition = new TickItem((s, g, e) =>
			{
				var time = Easing.EaseOut(s.TotalTime.TotalSeconds / SECONDS, EasingType.Quadratic);
				var newPos = 2 - (2 * time);
				if (newPos <= 0.01) {
					VportOffset = new Vector2(0, 0);
					s.RunLimit = s.Count;
					return;
				}
				VportOffset = new Vector2(newPos, VportOffset.Y);
			}, TimeSpan.Zero, true);
			inTransition.Retired += () => Transitioning = false;
			return inTransition;
		}

		private TickItem CreateOutTransition(string token) {
			var outTransition = new TickItem((s, g, e) =>
			{
				var time = Easing.EaseOut(s.TotalTime.TotalSeconds / SECONDS, EasingType.Quadratic);
				var newPos = 2 * time;
				if (2 - newPos <= 0.01) {
					VportOffset = new Vector2(2, 0);
					s.RunLimit = s.Count;
					return;
				}
				VportOffset = new Vector2(newPos, VportOffset.Y);
			}, TimeSpan.Zero, true);
			outTransition.Retired += () => { StateMachine.Transition(token); Transitioning = false; };
			return outTransition;
		}

		#endregion Private Methods
	}
}