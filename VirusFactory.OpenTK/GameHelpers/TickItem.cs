using OpenTK;
using System;

namespace VirusFactory.OpenTK.GameHelpers {

    public class TickItem {
        public Action<GameWindow, FrameEventArgs> Action;
        public TimeSpan WaitTime;
        public DateTime LastCall = DateTime.MinValue;
        public TimeSpan Elapsed;
        public int RunLimit, Count;

        public TickItem(Action<GameWindow, FrameEventArgs> action, TimeSpan waitTime, bool runImmediately = false, int runLimit = -1) {
            Action = action;
            WaitTime = waitTime;
            RunLimit = runLimit;
            Elapsed = runImmediately ? waitTime : TimeSpan.Zero;
        }
    }
}