using OpenTK;
using System;
using System.Collections.Generic;

namespace VirusFactory.OpenTK.GameHelpers {

    public class TickScheduler {
        private readonly List<TickItem> _retiredItems = new List<TickItem>();

        public List<TickItem> TickItems { get; } = new List<TickItem>();

        public IReadOnlyList<TickItem> RetiredItems => _retiredItems;

        public void Tick(GameWindow sender, FrameEventArgs e) {
            var toRetire = new List<TickItem>();
            foreach (var tickItem in TickItems) {
                if (tickItem.WaitTime == TimeSpan.Zero)
                    RunTick(sender, e, tickItem, toRetire);
                else {
                    tickItem.Elapsed = new TimeSpan(tickItem.Elapsed.Ticks + (long)(e.Time * 1E+07));
                    while (tickItem.WaitTime != TimeSpan.Zero && tickItem.Elapsed >= tickItem.WaitTime) {
                        if (RunTick(sender, e, tickItem, toRetire)) break;
                        tickItem.Elapsed -= tickItem.WaitTime;
                    }
                }
            }
            toRetire.ForEach(o => { TickItems.Remove(o); o.Retire(); });
            _retiredItems.AddRange(toRetire);
        }

        private static bool RunTick(GameWindow sender, FrameEventArgs e, TickItem tickItem, List<TickItem> toRetire) {
            tickItem.TotalTime += TimeSpan.FromSeconds(e.Time);
            tickItem.Action.Invoke(tickItem, sender, e);
            tickItem.LastCall = DateTime.Now;
            if (tickItem.RunLimit <= 0 | ++tickItem.Count < tickItem.RunLimit) return false;
            toRetire.Add(tickItem);
            return true;
        }
    }
}