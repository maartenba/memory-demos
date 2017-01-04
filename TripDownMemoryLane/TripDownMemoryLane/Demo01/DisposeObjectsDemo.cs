using System;
using System.Collections.Generic;
using System.IO;

namespace TripDownMemoryLane.Demo01
{
    public class DisposeObjectsDemo
        : IDemo
    {
        public void Run(string[] args)
        {
            File.WriteAllText("disposeobjectsdemo.txt", "Hello.");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Disposable objects");
            Console.ResetColor();

            Console.WriteLine("Help the garbage collector to clean up objects!");
            Console.WriteLine("Disposable does just that.");

            DemoDontDispose();
            DemoDispose();
        }

        private static void DemoDontDispose()
        {
            Console.WriteLine("Let's generate 10.000 objects and not dispose them. Let's show what happens when not disposing... (see SampleDisposable)");

            var disposables = new List<SampleDisposable>();
            for (int i = 0; i < 10000; i++)
            {
                disposables.Add(
                    new SampleDisposable(new FileStream("disposeobjectsdemo.txt", FileMode.OpenOrCreate, FileAccess.Read)));
            }

            Console.WriteLine("Collect a snapshot, then press enter to run GC.");
            Console.ReadLine();
            disposables.Clear();
            GC.Collect(0);

            Console.WriteLine("Collect a snapshot, and see if there are any SampleDisposable in memory.");
            Console.WriteLine("All objects are in the finalizer queue... We need another GC! (enter)");
            Console.ReadLine();
            GC.Collect(0);
            GC.Collect(1);

            Console.WriteLine("Collect a snapshot, objects are now really gone.");
            Console.ReadLine();
            GC.Collect(0);
            GC.Collect(1);
        }

        private static void DemoDispose()
        {
            Console.WriteLine("Let's generate 10.000 objects and this time, dispose them.");

            var disposables = new List<SampleDisposable>();
            for (int i = 0; i < 10000; i++)
            {
                disposables.Add(
                    new SampleDisposable(new FileStream("disposeobjectsdemo.txt", FileMode.OpenOrCreate, FileAccess.Read)));
            }

            Console.WriteLine("Collect a snapshot, then press enter to dispose all objects and run GC.");
            Console.ReadLine();

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
            disposables.Clear();
            GC.Collect(0);

            Console.WriteLine("Collect a snapshot, and see if there are any SampleDisposable in memory. They should be gone now.");
            GC.Collect(0);
        }
    }
}