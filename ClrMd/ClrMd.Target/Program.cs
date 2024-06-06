using System;
using System.Threading;
using ClrMd.Target;

string helloWorld = "Hello, world!";

Console.WriteLine(helloWorld);

Timer timer;
using (Clock clock = new Clock())
{
    timer = new Timer(clock.OnTick,
        null,
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(1));
}

GC.Collect(0);
Console.WriteLine("Performed GC.Collect(0).");

Console.WriteLine("Press <enter> to quit");
Console.ReadLine();