using OpenTK;
using System;

namespace VirusFactory.OpenTK.GameHelpers {
	public class TickItem {
		#region Public Fields

		public Action<TickItem, GameWindow, FrameEventArgs> Action;
		public TimeSpan WaitTime;
		public DateTime LastCall = DateTime.MinValue;
		public DateTime StartTime;
		public TimeSpan Elapsed, TotalTime = TimeSpan.Zero;
		public int RunLimit, Count;

		#endregion Public Fields

		#region Public Events + Delegates

		public event Action Retired;

		#endregion Public Events + Delegates

		#region Public Ctor / Dtor

		public TickItem(Action<TickItem, GameWindow, FrameEventArgs> action, TimeSpan waitTime, bool runImmediately = false, int runLimit = -1) {
			Action = action;
			WaitTime = waitTime;
			RunLimit = runLimit;
			Elapsed = runImmediately ? waitTime : TimeSpan.Zero;
		}

		#endregion Public Ctor / Dtor

		#region Public Methods

		public void Retire() {
			Retired?.Invoke();
		}

		#endregion Public Methods
	}
}