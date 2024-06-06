using System;

namespace ClrMd.Target;

class Clock
    : IDisposable
{
    public void OnTick(object state)
    {
        Console.WriteLine(DateTime.UtcNow);
    }

    public void Dispose()
    {
    }
}