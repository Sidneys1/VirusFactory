using System;
using System.Collections.Generic;

namespace VirusFactory.OpenTK.GameHelpers.Behaviourals {
    public interface IBehaviored<TTrigger, TSubclass> where TTrigger : struct, IConvertible where TSubclass : IBehaviored<TTrigger, TSubclass> {
        MultiMap<TTrigger, BehaviourBase<TTrigger, TSubclass>> Behaviours { get; }
        Dictionary<object, object> AttachedProperties { get; } 
        void Trigger(TTrigger trigger);
    }
}
