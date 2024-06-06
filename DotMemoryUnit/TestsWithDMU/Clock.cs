using System;
using System.Timers;

namespace TestsWithDMU;

class Clock
    : IDisposable
{
    public void OnTick(object state, ElapsedEventArgs args)
    {
        Console.WriteLine(DateTime.UtcNow);
    }

    public void Dispose()
    {
    }
}