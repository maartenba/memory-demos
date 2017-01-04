using System;

namespace TripDownMemoryLane.Demo01
{
    public class WeakReferenceDemo
        : IDemo
    {
        public void Run(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Weak references");
            Console.ResetColor();
            Console.WriteLine("Weak references allow the Garbage Collector to reclaim a referenced object.");
            Console.WriteLine("Let's generate a cache of 20 objects with a weak reference. (see Cache)");

            var r = new Random();
            var c = new Cache(20);

            Console.WriteLine("Collect a snapshot, and see if there are any WeakReference<Data> in memory. There should be.");
            Console.WriteLine("Then press enter to run GC.");
            Console.ReadLine();
            GC.Collect(0);

            // Randomly access objects in the cache.
            string dummy = "";
            for (int i = 0; i < c.Count; i++)
            {
                int index = r.Next(c.Count);

                // Access the object by getting a property value.
                dummy = c[index].Name;
            }

            // Show results
            double regenPercent = c.RegenerationCount / (double)c.Count;
            Console.WriteLine("Cache size: {0}, Regenerated: {1:P2}", c.Count, regenPercent);
            Console.WriteLine();

            // Collect another snapshot
            Console.WriteLine("Collect a snapshot, compare both snapshots. We should see new WeakReference<Data> being generated.");
            Console.ReadLine();
        }
    }
}