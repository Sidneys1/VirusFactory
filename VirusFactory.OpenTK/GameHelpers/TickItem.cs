using OpenTK;
using System;

namespace VirusFactory.OpenTK.GameHelpers {

    public class TickItem {
        public Action<TickItem, GameWindow, FrameEventArgs> Action;
        public TimeSpan WaitTime;
        public DateTime LastCall = DateTime.MinValue;
        public DateTime StartTime;
        public TimeSpan Elapsed, TotalTime = TimeSpan.Zero;
        public int RunLimit, Count;

        public event Action Retired;

        public TickItem(Action<TickItem, GameWindow, FrameEventArgs> action, TimeSpan waitTime, bool runImmediately = false, int runLimit = -1) {
            Action = action;
            WaitTime = waitTime;
            RunLimit = runLimit;
            Elapsed = runImmediately ? waitTime : TimeSpan.Zero;
        }

        public void Retire() {
            Retired?.Invoke();
        }
    }
}