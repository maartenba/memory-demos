using System.Runtime.InteropServices;

namespace TripDownMemoryLane.Demo05;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Heap_segment
{
    public nint allocated;
    public nint committed;
    public nint reserved;
    public nint used;
    public nint mem;
}