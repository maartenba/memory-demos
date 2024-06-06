using System;
using System.Threading;
using ExampleTestsWithDMU;
using JetBrains.dotMemoryUnit;
using Xunit;
using Xunit.Abstractions;
using Timer = System.Timers.Timer;

namespace TestsWithDMU
{
    public class ClockFacts
    {
        public ClockFacts(ITestOutputHelper outputHelper)
        {
            DotMemoryUnitTestOutput.SetOutputMethod(
                message => outputHelper.WriteLine(message));
        }

        [Fact]
        public void ClockDisposesCorrectly()
        {
            var isolator = new Action(() =>
            {
                // Arrange
                Timer timer;
                using (Clock clock = new Clock())
                {
                    // Act
                    timer = new Timer(1000);
                    timer.Elapsed += clock.OnTick;
                    timer.Start();

                    Thread.Sleep(5 * 1000); // Run clock for 5 seconds
                }
            });

            isolator();

            // Run explicit GC
            GC.Collect();

            // Assert
            dotMemory.Check(memory => 
                Assert.Equal(0, memory.GetObjects(where => where.Type.Is<Clock>()).ObjectsCount));
        }

        [Fact]
        public void ClockDisposesCorrectlyFixed()
        {
            var isolator = new Action(() =>
            {
                // Arrange
                using (Clock clock = new Clock())
                using (Timer timer = new Timer(1000))
                {
                    // Act
                    timer.Elapsed += clock.OnTick;
                    timer.Start();

                    Thread.Sleep(5 * 1000); // Run clock for 5 seconds

                    timer.Stop();
                    timer.Elapsed -= clock.OnTick;
                }
            });

            isolator();

            // Run explicit GC
            GC.Collect();

            // Assert
            dotMemory.Check(memory =>
            {
                Assert.Equal(0, memory.GetObjects(where => where.Type.Is<Clock>()).ObjectsCount);
            });
        }
    }
}
