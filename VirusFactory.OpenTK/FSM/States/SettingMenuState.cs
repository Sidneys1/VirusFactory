using OpenTK;
using Behaviorals;
using OpenTK.Graphics;
using QuickFont;
using VirusFactory.OpenTK.FSM.Behaviours;
using VirusFactory.OpenTK.FSM.Elements;
using VirusFactory.OpenTK.FSM.States.Base;

namespace VirusFactory.OpenTK.FSM.States {
    public class SettingMenuState : MenuStateBase {
        #region Ctor / Dtor

        public SettingMenuState(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
            var uiElementBehavior = new Behaviour<UiElement>(o =>
            {
                var floatPos = BehaviorHelpers.EaseMouse(o.MousePosition);
                o.PositionAdd = floatPos / ((float)o.AttachedProperties["floatiness"]);
            });
            var exitButton = new TextElement(owner, "return", ".\\fonts\\toxica.ttf", 40f) {
                NormalColor = Color4.Gray,
                Position = new Vector2(0f, -0.15f),
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 50f } },
                MouseOverColor = Color4.White,
            };
            exitButton.Clicked += args => TransitionOut("return");

            GameElements.Add(new TextElement(owner, "Settings", ".\\fonts\\toxica.ttf", 72f) {
                NormalColor = Color4.DarkRed,
                Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
                AttachedProperties = { { "floatiness", 25f } },
                Position = new Vector2(0f, -0.75f)
            });

            GameElements.Add(new TextElement(owner, "alpha 0.1", ".\\fonts\\pixelmix.ttf", 12f) {
                Position = new Vector2(0.8f, 0.9f),
                Alignment = QFontAlignment.Left
            });
            
            GameElements.Add(exitButton);
        }

        #endregion Ctor / Dtor
    }
}