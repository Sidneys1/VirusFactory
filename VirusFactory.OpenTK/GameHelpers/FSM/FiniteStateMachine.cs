using System.Collections.Generic;
using System.Linq;

namespace VirusFactory.OpenTK.GameHelpers.FSM {
    public abstract class FiniteStateMachine<T> where T : State {
        public List<T> States { get; } = new List<T>();
        public T CurrState { get; set; }

        public void Transition(T to) {
            if (CurrState == null && States.Contains(to) && to.ToThisTransitions.Contains(new Transition(Command.Deactivate, to.GetType(), null))) {
                CurrState = to;
                return;
            }

            var transition = CurrState?.FromThisTransitions.FirstOrDefault(o => o.To != typeof(T));
            if (transition == null)
                throw new InvalidTransitionException($"Transition '{CurrState} -> {to}' is invalid.");

            CurrState.TransitionFrom(transition);
            to.TransitionTo(transition);
            CurrState = to;
        }
    }
}
