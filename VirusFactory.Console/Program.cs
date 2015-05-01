using System.Diagnostics;
using VirusFactory.Model.Geography;
using static System.Console;

namespace VirusFactory.Console {
	class Program {
		static void Main() {
		    var w = new Stopwatch();
            w.Start();
		    var world = World.Load("world.dat");
            w.Stop();
		    WriteLine($"New Method: {w.ElapsedMilliseconds:N0}ms.");

            w.Start();
            world = new World("countries.dat", "cities");
            w.Stop();
            WriteLine($"Old Method: {w.ElapsedMilliseconds:N0}ms.");
            ReadLine();
		}
	}
}
