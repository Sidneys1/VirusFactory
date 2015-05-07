using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GFSM;
using OpenTK;
using OpenTK.Graphics;
using VirusFactory.OpenTK.FSM;
using VirusFactory.OpenTK.FSM.States;
using VirusFactory.OpenTK.FSM.States.Base;
// ReSharper disable AccessToDisposedClosure

namespace VirusFactory.OpenTK {
	internal class Program {
		#region Methods

		private static void Main() {
			var missingfiles = new[] {
				".\\world.dat",
				".\\fonts\\toxica.ttf",
				".\\fonts\\pixelmix.ttf",
				".\\fonts\\pixelmix_bold.ttf",
				".\\fonts\\pixelmix_micro.ttf"
			}.Where(file => !File.Exists(file)).ToList();

			foreach (var file in missingfiles) {
				MessageBox.Show(
					$"The file {Environment.NewLine}{new FileInfo(file).FullName}{Environment.NewLine}could not be found.",
					"A required file is missing!");
			}
			if (missingfiles.Any())
				return;
			
			using (var game = new GameWindow(1280, 720, new GraphicsMode(32, 24, 0, 8))) {
				var fsm = new GameFiniteStateMachine();
				GameStateBase mainMenuState = new MainMenuState(game, fsm);
				GameStateBase ingameState = new IngameState(game, fsm);
				GameStateBase settingsMenuState = new SettingMenuState(game, fsm);
				GameStateBase pauseMenuState = new PauseMenuState(game, fsm);

				fsm.States.Add(mainMenuState);
				fsm.States.Add(ingameState);
				fsm.States.Add(settingsMenuState);
				fsm.States.Add(pauseMenuState);

				fsm.AddTransition(new Transition<GameStateBase>("init", null, mainMenuState));
				fsm.AddTransition(new Transition<GameStateBase>("start", mainMenuState, ingameState));
				fsm.AddTransition(new Transition<GameStateBase>("settings", mainMenuState, settingsMenuState));
				fsm.AddTransition(new Transition<GameStateBase>("settings", pauseMenuState, settingsMenuState));
				fsm.AddTransition(new Transition<GameStateBase>("pause", ingameState, pauseMenuState));
				fsm.AddTransition(new Transition<GameStateBase>("main menu", pauseMenuState, mainMenuState, Mode.Pop));

				fsm.AddTransition(new Transition<GameStateBase>("return", settingsMenuState, mainMenuState, Mode.Pop));
				fsm.AddTransition(new Transition<GameStateBase>("return", settingsMenuState, pauseMenuState, Mode.Pop));
				fsm.AddTransition(new Transition<GameStateBase>("return", pauseMenuState, ingameState, Mode.Pop));

				fsm.AddTransition(new Transition<GameStateBase>("exit", mainMenuState, null));
				fsm.AddTransition(new Transition<GameStateBase>("exit", pauseMenuState, null));

				fsm.Transitioned += t => { if (t.To == null) game.Exit(); };

				fsm.Transition("init");

				game.Load += (sender, args) => fsm.Load();
				game.Unload += (sender, args) => fsm.UnLoad();

				game.KeyDown += (sender, args) => fsm.KeyDown(args);
				game.KeyUp += (sender, args) => fsm.KeyUp(args);
				game.KeyPress += (sender, args) => fsm.KeyPress(args);

				game.MouseMove += (sender, args) => fsm.MouseMove(args);
				game.MouseDown += (sender, args) => fsm.MouseDown(args);
				game.MouseUp += (sender, args) => fsm.MouseUp(args);
				game.MouseEnter += (sender, args) => fsm.MouseEnter();
				game.MouseLeave += (sender, args) => fsm.MouseLeave();
				game.MouseWheel += (sender, args) => fsm.MouseWheel(args);

				game.Resize += (sender, args) => fsm.Resize();
				game.RenderFrame += (sender, args) => fsm.RenderFrame(args);
				game.UpdateFrame += (sender, args) => fsm.UpdateFrame(args);

				game.Title = "Apoplexy";
				game.Icon = new Icon("Grim Reaper.ico");

				game.Run(30, 60);
			}
		}
		
		#endregion Methods
	}
}