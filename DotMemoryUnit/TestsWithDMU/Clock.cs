using System;
using System.Timers;

namespace ExampleTestsWithDMU
{
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
}