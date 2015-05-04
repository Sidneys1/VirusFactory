using System.Diagnostics;
using VirusFactory.Model.Geography;
using static System.Console;

namespace VirusFactory.Console {

    internal class Program {

        private static void Main() {
            var w = new Stopwatch();
            w.Start();
            var world = World.Load("world.dat");
            w.Stop();
            WriteLine($"New Method: {w.ElapsedMilliseconds:N0}ms.");

            w.Restart();
            world = new World("countries.dat", "cities");
            w.Stop();
            WriteLine($"Old Method: {w.ElapsedMilliseconds:N0}ms.");
            ReadLine();
        }
    }
}