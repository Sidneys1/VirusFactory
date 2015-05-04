﻿using System;

namespace VirusFactory.OpenTK.GameHelpers.Behaviourals {
    public abstract class BehaviourBase<TTrigger, TSubclass> where TTrigger : struct, IConvertible where TSubclass : IBehaviored<TTrigger, TSubclass> {
        protected BehaviourBase(Action<TSubclass> action) {
            if (!typeof(TTrigger).IsEnum)
                throw new ArgumentException($"Generic parameter {nameof(TTrigger)} must be an enum");

            Action = action;
        }

        public Action<TSubclass> Action { get; protected set; }
    }
}
