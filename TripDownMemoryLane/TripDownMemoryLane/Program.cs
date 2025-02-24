using System;
using System.Collections.Generic;
using System.Linq;
using TripDownMemoryLane;
using TripDownMemoryLane.Demo01;
using TripDownMemoryLane.Demo02;
using TripDownMemoryLane.Demo04;
using TripDownMemoryLane.Demo05;

var availableDemos = new Dictionary<string, Type[]>
{
    {"1",   new[] {typeof(WeakReferenceDemo), typeof(DisposeObjectsDemo)}},
    {"1-1", new[] {typeof(WeakReferenceDemo)}},
    {"1-2", new[] {typeof(DisposeObjectsDemo)}},
    {"2",   new[] {typeof(TupleVsValueTupleDemo)}},
    {"3",   new[] {typeof(TripDownMemoryLane.Demo03.NewtonsoftJson.BeersDemoInsane), typeof(TripDownMemoryLane.Demo03.NewtonsoftJson.BeersDemoUnoptimized), typeof(TripDownMemoryLane.Demo03.NewtonsoftJson.BeersDemoOptimized), typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoInsane), typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoUnoptimized), typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoOptimized)}},
    {"3-1", new[] {typeof(TripDownMemoryLane.Demo03.NewtonsoftJson.BeersDemoInsane)}},
    {"3-2", new[] {typeof(TripDownMemoryLane.Demo03.NewtonsoftJson.BeersDemoUnoptimized)}},
    {"3-3", new[] {typeof(TripDownMemoryLane.Demo03.NewtonsoftJson.BeersDemoOptimized)}},
    {"3-6", new[] {typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoInsane), typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoUnoptimized), typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoOptimized)}},
    {"3-7", new[] {typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoInsane)}},
    {"3-8", new[] {typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoUnoptimized)}},
    {"3-9", new[] {typeof(TripDownMemoryLane.Demo03.SystemTextJson.BeersDemoOptimized)}},
    {"4",   new[] {typeof(StringAllocationDemo)}},
    {"5",   new[] {typeof(FrozenSegmentsDemo)}},
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