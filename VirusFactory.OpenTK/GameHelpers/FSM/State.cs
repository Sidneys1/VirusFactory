using System.Linq;

namespace VirusFactory.OpenTK.GameHelpers.FSM {
    public abstract class State {
        public StateMode Mode { get; private set; }
        public abstract Transition[] ToThisTransitions { get; }
        public abstract Transition[] FromThisTransitions { get; }

        protected State(StateMode initialMode) {
            Mode = initialMode;
        }

        public void TransitionFrom(Transition transition) {
            if (!FromThisTransitions.Contains(transition))
                throw new InvalidTransitionException($"Transition '{transition}' is invalid.");

            switch (transition.Command) {
                case Command.Deactivate:
                    Exit();
                    break;
                case Command.Pause:
                    Pause();
                    break;
            }
        }

        public void TransitionTo(Transition transition) {
            if (!ToThisTransitions.Contains(transition))
                throw new InvalidTransitionException($"Transition '{transition}' is invalid.");

            Enter();
        }

        public virtual void Enter() {
        }

        public virtual void Pause() {
        }

        public virtual void Exit() {
        }
    }
}
