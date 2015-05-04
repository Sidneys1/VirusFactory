using OpenTK;
using System;
using GFSM;

namespace VirusFactory.OpenTK.FSM.States {

    public class PauseMenuState : GameStateBase {

        public PauseMenuState(GameWindow owner, GameFiniteStateMachine parent) : base(owner, parent) {
        }

        public override Transition[] ToThisTransitions { get; }

        public override Transition[] FromThisTransitions { get; }

        public override void Enter() {
            throw new NotImplementedException();
        }

        public override void Exit() {
            throw new NotImplementedException();
        }
    }
}