using System;

namespace TripDownMemoryLane.Demo05;

/// <summary>
/// Kudos to Kevin Gosse for this example!
/// https://minidump.net/exploring-frozen-segments/
/// </summary>
public class FrozenSegmentsDemo
    : IDemo
{
    public class MyObject
    {
        public long data;
    }

    public void Run(string[] args)
    {
        Console.WriteLine("Capture a snapshot and explore frozen segments. There should be none (at least none that we created).");
        Console.WriteLine("Hit enter to continue.");
        Console.ReadLine();

        using var allocator = new BumpPointerNativeAllocator(1024 * 1024 * 250);

        var array = allocator.AllocateArray<object>(4);

        array[0] = allocator.AllocateString(['H', 'e', 'l', 'l', 'o']);

        var myObject1 = allocator.AllocateObject<MyObject>();
        myObject1.data = 42;
        array[1] = myObject1;

        var myObject2 = allocator.AllocateObject<MyObject>();
        myObject2.data = 1;
        array[2] = myObject2;

        array[3] = allocator.AllocateString(['w', 'o', 'r', 'l', 'd', '!']);

        foreach (var item in array)
        {
            if (item is string str)
            {
                Console.WriteLine(str);
            }
            else if (item is MyObject obj)
            {
                Console.WriteLine(obj.data);
            }
        }

        GC.Collect();

        Console.WriteLine("Capture a snapshot and explore frozen segments.");
        Console.WriteLine("Hit enter to quit.");
        Console.ReadLine();
    }
}