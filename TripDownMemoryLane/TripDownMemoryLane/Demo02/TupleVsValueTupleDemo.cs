using System;
using System.Collections.Generic;

namespace TripDownMemoryLane.Demo02;

public class TupleVsValueTupleDemo : IDemo
{
    public void Run(string[] args)
    {
            var tuples = new List<Tuple<int, int>>();
            var valueTuples = new List<ValueTuple<int, int>>();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Tuple vs. ValueTuple");
            Console.ResetColor();

            Console.WriteLine("Do objects always make sense?");
            Console.WriteLine("Attach the memory profiler, and see how they behave...");

            for (var i = 0; i < 1000; i++)
            {
                tuples.Add(CreateTuple());
            }
            Console.WriteLine("Collect a snapshot, and see how many Tuple<string, string> are there. There should be 1000.");
            Console.ReadLine();

            for (var i = 0; i < 1000; i++)
            {
                valueTuples.Add(CreateValueTuple());
            }
            Console.WriteLine("Collect a snapshot, and see how many ValueTuple<string, string> are there. There should be none (on the heap), as they get allocated on the stack.");
            Console.ReadLine();

            Console.WriteLine("Tuple are allocated on the heap, ValueTuple on the stack.");
            Console.WriteLine("* No allocations happening with ValueTuple");
            Console.WriteLine("    (referenced objects would still be on heap!)");
            Console.WriteLine("* GC is needed for Tuple, not for ValueTuple");
            Console.WriteLine("* Less work for GC, less pauses!");
            Console.WriteLine("* Tuple not really limited in available memory space");
            Console.WriteLine("* ValueTuple limited in available memory space");
        }

    public Tuple<int, int> CreateTuple()
    {
            return new Tuple<int, int>(123, 321);
        }

    public (int, int) CreateValueTuple()
    {
            return (123, 321);
        }
}