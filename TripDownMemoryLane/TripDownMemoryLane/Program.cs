using System;
using System.Collections.Generic;
using System.Linq;
using TripDownMemoryLane.Demo01;
using TripDownMemoryLane.Demo02;
using TripDownMemoryLane.Demo03;
using TripDownMemoryLane.Demo04;

namespace TripDownMemoryLane
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var availableDemos = new Dictionary<string, Type[]>
            {
                {"1",   new[] {typeof(WeakReferenceDemo), typeof(DisposeObjectsDemo)}},
                {"1-1", new[] {typeof(WeakReferenceDemo)}},
                {"1-2", new[] {typeof(DisposeObjectsDemo)}},
                {"2",   new[] {typeof(TupleVsValueTupleDemo)}},
                {"3",   new[] {typeof(BeersDemoUnoptimized), typeof(BeersDemoOptimized)}},
                {"3-1", new[] {typeof(BeersDemoUnoptimized)}},
                {"3-2", new[] {typeof(BeersDemoOptimized)}},
                {"4",   new[] {typeof(StringAllocationDemo)}}
            };

            Console.WriteLine("Available demo's:");
            foreach (var keyValuePair in availableDemos)
            {
                Console.WriteLine(" * " + keyValuePair.Key + " - " + string.Join(", ", keyValuePair.Value.Select(t => t.Name)));
            }

            Console.Write("Which demo would you like to run? (enter a number) ");
            var demoNumber = Console.ReadLine()?.TrimEnd('\r', 'n') ?? string.Empty;
            if (availableDemos.TryGetValue(demoNumber, out var demosToRun))
            {
                // Run and make sure to clean up
                foreach (var demo in demosToRun)
                {
                    GC.Collect();
                    Console.Clear();

                    ((IDemo)Activator.CreateInstance(demo)).Run(args);

                    Console.WriteLine();
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Press <enter> to continue.");
                    Console.ResetColor();
                    Console.ReadLine();
                }
            }
            else
            {
                // No can do!
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unknown demo. Try restarting the application.");
                Console.ResetColor();
                Console.ReadLine();
            }
        }
    }
}