using System;
using OpenTK;
using VirusFactory.OpenTK.GameHelpers.FSM;

namespace VirusFactory.OpenTK.FSM.States {
    public class IngameState : GameStateBase {
        public IngameState(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
            ToThisTransitions = new[] {new Transition(Command.Deactivate, typeof(IngameState), typeof(MainMenuState)) };
        }

        public override Transition[] ToThisTransitions { get; }
        public override Transition[] FromThisTransitions { get; }
    }
}
