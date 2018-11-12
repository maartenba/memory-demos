using System;
using System.Threading;

namespace ClrMd.Target
{
    class Program
    {
        static void Main(string[] args)
        {
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
        }
    }

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
}
