using System.Runtime.InteropServices;

namespace TripDownMemoryLane.Demo05;

[StructLayout(LayoutKind.Explicit)]
public struct MethodTable
{
    [FieldOffset(0)] public ushort ComponentSize;

    [FieldOffset(4)] public int BaseSize;
}