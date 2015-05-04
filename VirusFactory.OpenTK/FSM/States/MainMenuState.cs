using MoreLinq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using System.Linq;
using VirusFactory.OpenTK.FSM.Behaviours;
using VirusFactory.OpenTK.FSM.Elements;
using VirusFactory.OpenTK.FSM.Interface;
using VirusFactory.OpenTK.GameHelpers;
using VirusFactory.OpenTK.GameHelpers.Behaviourals;
using VirusFactory.OpenTK.GameHelpers.FSM;

namespace VirusFactory.OpenTK.FSM.States {

    public class MainMenuState : GameStateBase, IInputtable, IUpdateable {

        #region Properties

        public override Transition[] ToThisTransitions { get; }
        = {
            new Transition(Command.Deactivate, typeof(MainMenuState), typeof(PauseMenuState)),
            new Transition(Command.Deactivate, typeof(MainMenuState), null)
        };

        public override Transition[] FromThisTransitions { get; }
        = {
            new Transition(Command.Deactivate, typeof(IngameState), typeof(MainMenuState)),
            new Transition(Command.Deactivate, null, typeof(MainMenuState))
        };

        #endregion Properties

        #region Constructors

        public MainMenuState(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
            var uiElementBehavior = new Behaviour<GameTriggers, UiElement>(o =>
            {
                var floatPos = EaseMouse(o.MousePosition);
                o.PositionAdd = floatPos / ((float)o.AttachedProperties["floatiness"]);
            });

            var startButton = new TextElement(owner, "start", ".\\fonts\\toxica.ttf", 40f) {
                NormalColor = Color4.Gray,
                Position = new Vector2(0f, -0.4f),
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 50f } },
                MouseOverColor = Color4.White
            };

            startButton.Clicked += args =>
            {
                StateMachine.Transition(parent.States.OfType<IngameState>().FirstOrDefault() ?? new IngameState(Owner, StateMachine));
            };

            var exitButton = new TextElement(owner, "exit", ".\\fonts\\toxica.ttf", 40f) {
                NormalColor = Color4.Gray,
                Position = new Vector2(0f, -0.15f),
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 50f } },
                MouseOverColor = Color4.White,
            };
            exitButton.Clicked += args => owner.Exit();

            GameElements.Add(new TextElement(owner, "Apoplexy", ".\\fonts\\toxica.ttf", 72f) {
                NormalColor = Color4.DarkRed,
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 25f } },
                Position = new Vector2(0f, -0.75f)
            });

            GameElements.Add(new TextElement(owner, "alpha 0.1", ".\\fonts\\pixelmix.ttf", 12f) {
                Position = new Vector2(0.8f, 0.9f),
                Alignment = QFontAlignment.Left
            });

            GameElements.Add(startButton);
            GameElements.Add(exitButton);
        }

        #endregion Constructors

        #region Methods

        public override void RenderFrame(FrameEventArgs e) {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.PopAttrib();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(-1, 1, -1, 1, 0.0, 4.0);

            base.RenderFrame(e);

            Owner.SwapBuffers();
        }

        #endregion Methods

        #region IInputtable

        void IInputtable.KeyDown(KeyboardKeyEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.KeyDown(e));

            if (e.Key == Key.Escape)
                Owner.Exit();
        }

        void IInputtable.KeyPress(KeyPressEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.KeyPress(e));
        }

        void IInputtable.KeyUp(KeyPressEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.KeyUp(e));
        }

        void IInputtable.MouseDown(MouseButtonEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseDown(e));
        }

        void IInputtable.MouseUp(MouseButtonEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseUp(e));
        }

        void IInputtable.MouseEnter() {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseEnter());
        }

        void IInputtable.MouseLeave() {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseLeave());
        }

        void IInputtable.MouseMove(MouseMoveEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseMove(e));
        }

        void IInputtable.MouseWheel(MouseWheelEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseWheel(e));
        }

        #endregion IInputtable

        #region IUpdateable

        void IUpdateable.UpdateFrame(FrameEventArgs e) {
            GameElements.OfType<IUpdateable>().ForEach(o => o.UpdateFrame(e));
        }

        #endregion IUpdateable

        #region Static Helpers

        private static Vector2 EaseMouse(Vector2 t) {
            return new Vector2(EaseMouse(t.X), EaseMouse(t.Y));
        }

        private static float EaseMouse(float t) {
            if (t < 0)
                return -Easing.EaseOut(-t, EasingType.Quadratic);
            return Easing.EaseOut(t, EasingType.Quadratic);
        }

        #endregion Static Helpers
    }
}