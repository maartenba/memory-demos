using System;

namespace TripDownMemoryLane.Demo03.SystemTextJson;

public class BeersDemoOptimized
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
                BeerLoader.LoadBeersOptimized();

                Console.WriteLine("Collect a snapshot, press enter to run GC.");
                Console.ReadLine();
            }

            Console.WriteLine("Now look at the snapshots in the profiler...");
            Console.WriteLine("* GC is almost invisible");
            Console.WriteLine("* Less allocations happening");
            Console.WriteLine("* Compare two snapshots: almost no traffic");
            Console.WriteLine("* Less work for GC, less pauses!");
        }
}