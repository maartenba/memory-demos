using System;

namespace TripDownMemoryLane.Demo03.SystemTextJson;

public class BeersDemoInsane
    : IDemo
{
    public void Run(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Loading beers!");
        Console.ResetColor();
        Console.WriteLine("Attach the memory profiler, and load beers a number of times.");

        for (var i = 0; i < 2; i++)
        {
            BeerLoader.LoadBeersInsane();

            Console.WriteLine("Collect a snapshot, press enter to run GC.");
            Console.ReadLine();
        }

        Console.WriteLine("Now look at the snapshots in the profiler...");
        Console.WriteLine("* GC is very visible");
        Console.WriteLine("* Most memory in gen2 (we keep our beers around)");
        Console.WriteLine("* Compare two snapshots: high traffic on strings (as expected)");
        Console.WriteLine("* (Lots of string allocations - original stream, but mo more when deserializing, already faster/more efficient than Newtonsoft.Json)");
    }
}