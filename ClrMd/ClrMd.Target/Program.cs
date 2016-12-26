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

            using (Clock clock = new Clock())
            {
                Timer timer = new Timer(clock.OnTick,
                    null,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1));
            }

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
