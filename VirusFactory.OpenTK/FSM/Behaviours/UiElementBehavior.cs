using System;
using VirusFactory.OpenTK.GameHelpers.Behaviourals;

namespace VirusFactory.OpenTK.FSM.Behaviours {
    public class UiElementBehavior : BehaviourBase<GameTriggers, IBehavioredUiElement> {
        public UiElementBehavior(Action<IBehavioredUiElement> action) : base(action) {
        }
    }
}
