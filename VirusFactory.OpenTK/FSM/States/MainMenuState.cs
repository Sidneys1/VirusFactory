using System;
using System.Linq;
using MoreLinq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using QuickFont;
using VirusFactory.OpenTK.FSM.Elements;
using VirusFactory.OpenTK.FSM.Interface;
using VirusFactory.OpenTK.GameHelpers.FSM;

namespace VirusFactory.OpenTK.FSM.States {
    public class MainMenuState : GameStateBase, IInputtable, IUpdateable {
        private readonly FloatTextElement _startButton, _exitButton;

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

        public MainMenuState(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
            _startButton = new FloatTextElement(owner, "start", ".\\fonts\\toxica.ttf", 40f) {
                Color = Color4.Gray,
                Position = new Vector2(0f, -0.4f),
                Floatiness = 50f,
                MouseOverColor = Color4.White
            };

            _startButton.Clicked += args =>
            {
                StateMachine.Transition(parent.States.OfType<IngameState>().FirstOrDefault() ?? new IngameState(Owner, StateMachine));
            };

            _exitButton = new FloatTextElement(owner, "exit", ".\\fonts\\toxica.ttf", 40f) {
                Color = Color4.Gray,
                Position = new Vector2(0f, -0.15f),
                Floatiness = 50f,
                MouseOverColor = Color4.White
            };

            _exitButton.Clicked += args => owner.Exit();

            GameElements.Add(new FloatTextElement(owner, "Apoplexy", ".\\fonts\\toxica.ttf", 72f) {
                Color = Color4.DarkRed,
                Position = new Vector2(0f, -0.75f)
            });

            GameElements.Add(new TextElement(owner, "alpha 0.1", ".\\fonts\\pixelmix.ttf", 12f) {
                Position = new Vector2(0.8f, 0.9f),
                Alignment = QFontAlignment.Left
            });

            GameElements.Add(_startButton);
            GameElements.Add(_exitButton);
        }



        #region IInputtable

        public void KeyDown(KeyboardKeyEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.KeyDown(e));

            if (e.Key == Key.Escape)
                Owner.Exit();
        }

        public void KeyPress(KeyPressEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.KeyPress(e));
        }

        public void KeyUp(KeyPressEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.KeyUp(e));
        }

        public void MouseDown(MouseButtonEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseDown(e));
        }

        public void MouseUp(MouseButtonEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseUp(e));
        }

        public void MouseEnter() {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseEnter());
        }

        public void MouseLeave() {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseLeave());
        }

        public void MouseMove(MouseMoveEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseMove(e));
        }

        public void MouseWheel(MouseWheelEventArgs e) {
            GameElements.OfType<IInputtable>().ForEach(o => o.MouseWheel(e));
        }

        #endregion

        public void UpdateFrame(FrameEventArgs e) {
            GameElements.OfType<IUpdateable>().ForEach(o => o.UpdateFrame(e));
        }
    }
}
