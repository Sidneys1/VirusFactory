using Behaviorals;
using OpenTK;
using OpenTK.Graphics;
using VirusFactory.OpenTK.FSM.Behaviours;
using VirusFactory.OpenTK.FSM.Elements;
using VirusFactory.OpenTK.FSM.Elements.Base;
using VirusFactory.OpenTK.FSM.States.Base;

namespace VirusFactory.OpenTK.FSM.States {
	public class PauseMenuState : MenuStateBase {
		#region Public Ctor / Dtor

		public PauseMenuState(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
			var uiElementBehavior = new Behaviour<UiElementBase>(o =>
			{
				var floatPos = BehaviorHelpers.EaseMouse(o.MousePosition);
				o.PositionAdd = floatPos / ((float)o.AttachedProperties["floatiness"]);
			});

			var resumeButton = new TextElement(owner, "resume", ".\\fonts\\toxica.ttf", 40f) {
				NormalColor = Color4.Gray,
				Position = new Vector2(0f, -0.4f),
				Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
				AttachedProperties = { { "floatiness", 50f } },
				MouseOverColor = Color4.White
			};

			resumeButton.Clicked += args => TransitionOut("return");

			var settingsButton = new TextElement(owner, "settings", ".\\fonts\\toxica.ttf", 40f) {
				NormalColor = Color4.Gray,
				Position = new Vector2(0f, -0.15f),
				Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
				AttachedProperties = { { "floatiness", 50f } },
				MouseOverColor = Color4.White,
			};
			settingsButton.Clicked += args => TransitionOut("settings");

			var mainMenuButton = new TextElement(owner, "main menu", ".\\fonts\\toxica.ttf", 40f) {
				NormalColor = Color4.Gray,
				Position = new Vector2(0f, 0.1f),
				Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
				AttachedProperties = { { "floatiness", 50f } },
				MouseOverColor = Color4.White,
			};
			mainMenuButton.Clicked += args => TransitionOut("main menu");

			var exitButton = new TextElement(owner, "exit", ".\\fonts\\toxica.ttf", 40f) {
				NormalColor = Color4.Gray,
				Position = new Vector2(0f, 0.35f),
				Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
				AttachedProperties = { { "floatiness", 50f } },
				MouseOverColor = Color4.White,
			};
			exitButton.Clicked += args => StateMachine.Transition("exit");

			GameElements.Add(new TextElement(owner, "Game Paused", ".\\fonts\\toxica.ttf", 72f) {
				NormalColor = Color4.DarkRed,
				Behaviours = { { GameTriggers.MouseMove, uiElementBehavior } },
				AttachedProperties = { { "floatiness", 25f } },
				Position = new Vector2(0f, -0.75f)
			});
			GameElements.Add(resumeButton);
			GameElements.Add(exitButton);
			GameElements.Add(settingsButton);
			GameElements.Add(mainMenuButton);
		}

		#endregion Public Ctor / Dtor
	}
}