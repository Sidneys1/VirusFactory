using Behaviorals;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using VirusFactory.OpenTK.FSM.Behaviours;
using VirusFactory.OpenTK.FSM.Elements;
using VirusFactory.OpenTK.FSM.States.Base;

namespace VirusFactory.OpenTK.FSM.States {
    public class MainMenuState : MenuStateBase {
        #region Ctor / Dtor

        public MainMenuState(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
            var uiElementBehavior = new Behaviour<UiElement>(o =>
            {
                var floatPos = BehaviorHelpers.EaseMouse(o.MousePosition);
                o.PositionAdd = floatPos / ((float)o.AttachedProperties["floatiness"]);
            });

            var startButton = new TextElement(owner, "start", ".\\fonts\\toxica.ttf", 40f) {
                NormalColor = Color4.Gray,
                Position = new Vector2(0f, -0.4f),
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 50f } },
                MouseOverColor = Color4.White
            };

            startButton.Clicked += args => {
                if (Transitioning) return;
                TransitionOut("start");
            };

            var settingsButton = new TextElement(owner, "settings", ".\\fonts\\toxica.ttf", 40f) {
                NormalColor = Color4.Gray,
                Position = new Vector2(0f, -0.15f),
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 50f } },
                MouseOverColor = Color4.White
            };
            settingsButton.Clicked += args => {
                if (Transitioning) return;
                TransitionOut("settings");
            };

            var exitButton = new TextElement(owner, "exit", ".\\fonts\\toxica.ttf", 40f) {
                NormalColor = Color4.Gray,
                Position = new Vector2(0f, 0.1f),
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 50f } },
                MouseOverColor = Color4.White
            };
            exitButton.Clicked += args => StateMachine.Transition("exit");

            GameElements.Add(new TextElement(owner, "Apoplexy", ".\\fonts\\toxica.ttf", 72f) {
                NormalColor = Color4.DarkRed,
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 25f } },
                Position = new Vector2(0f, -0.75f)
            });
            GameElements.Add(startButton);
            GameElements.Add(exitButton);
            GameElements.Add(settingsButton);
        }

        #endregion Ctor / Dtor
        
        #region Methods
        
        public override void KeyDown(KeyboardKeyEventArgs e) {
            if (e.Key == Key.Escape)
                StateMachine.Transition("exit");

            base.KeyDown(e);
        }
        
        #endregion Methods
    }
}