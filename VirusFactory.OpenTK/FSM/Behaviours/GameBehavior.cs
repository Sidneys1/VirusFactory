using System;

namespace VirusFactory.OpenTK.FSM.Behaviours {
    public abstract class GameBehavior : GameHelpers.Behaviourals.BehaviourBase<GameTriggers, IGameBehaviored> {
        protected GameBehavior(Action<IGameBehaviored> action) : base(action) {
            
        }
    }
}
