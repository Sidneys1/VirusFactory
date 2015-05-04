using System;

namespace VirusFactory.OpenTK.GameHelpers.FSM {
    public class InvalidTransitionException : Exception {
        public InvalidTransitionException(string message):base(message){
            
        }
    }
}