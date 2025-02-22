using System;
using System.Reflection;

namespace TripDownMemoryLane.Demo05;

public static class FrozenGC
{
    public static IntPtr RegisterFrozenSegment(IntPtr sectionAddress, nint sectionSize)
    {
        return (IntPtr)typeof(GC).GetMethod("_RegisterFrozenSegment", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [sectionAddress, sectionSize])!;
    }

    public static void UnregisterFrozenSegment(IntPtr segment)
    {
        typeof(GC).GetMethod("_UnregisterFrozenSegment", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [segment]);
    }
}