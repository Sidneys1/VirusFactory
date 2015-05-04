using System;
using System.Collections.Generic;

namespace VirusFactory.OpenTK.GameHelpers.Behaviourals {
    public interface IBehavioral<TTrigger, TSubclass> where TTrigger : struct, IConvertible where TSubclass : IBehavioral<TTrigger, TSubclass> {
        MultiMap<TTrigger, Behaviour<TTrigger, TSubclass>> Behaviours { get; }
        Dictionary<object, object> AttachedProperties { get; } 
        void Trigger(TTrigger trigger);
    }
}
