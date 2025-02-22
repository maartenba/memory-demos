using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace TripDownMemoryLane.Demo05;

public unsafe class BumpPointerNativeAllocator : IDisposable
{
    private readonly IntPtr _segment;
    private readonly long _limit;

    private long _address;

    public BumpPointerNativeAllocator(nint size)
    {
        _address = (IntPtr)NativeMemory.AlignedAlloc((nuint)size, 8);
        NativeMemory.Clear((void*)_address, (nuint)size);
        _segment = FrozenGC.RegisterFrozenSegment((IntPtr)_address, size);

        var segment = (Heap_segment*)_segment;
        segment->allocated = (IntPtr)_address;

        _limit = _address + size;
    }

    ~BumpPointerNativeAllocator()
    {
        Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        FrozenGC.UnregisterFrozenSegment(_segment);
        NativeMemory.AlignedFree((void*)_address);
        _address = 0;
    }

    private nint* ReserveMemory(int size)
    {
        ObjectDisposedException.ThrowIf(_address == 0, typeof(BumpPointerNativeAllocator));

        if (_address + size > _limit)
        {
            throw new OutOfMemoryException();
        }

        var objectAddress = Interlocked.Add(ref _address, size);

        if (objectAddress > _limit)
        {
            throw new OutOfMemoryException();
        }

        // TODO: Should use Interlocked operations for thread safety
        var segment = (Heap_segment*)_segment;
        segment->allocated += size;

        return (nint*)(objectAddress - size);
    }

    public T AllocateObject<T>() where T : class
    {
        var mt = typeof(T).TypeHandle.Value;
        var methodTable = *(MethodTable*)mt;

        var ptr = ReserveMemory(methodTable.BaseSize);

        // Write the header
        *ptr = 0;
        ptr++;

        // Write the mt
        *ptr = mt;

        return *(T*)&ptr;
    }
    
    public ref T AllocateStruct<T>() where T : struct
    {
        var mt = typeof(T).TypeHandle.Value;
        var methodTable = *(MethodTable*)mt;

        var ptr = ReserveMemory(methodTable.BaseSize);

        return ref Unsafe.AsRef<T>(ptr);
    }

    public string AllocateString(ReadOnlySpan<char> data)
    {
        var mt = typeof(string).TypeHandle.Value;
        var methodTable = *(MethodTable*)mt;

        var size = methodTable.BaseSize + (data.Length + 1) * sizeof(char);

        // Align up the size
        size = (size + IntPtr.Size - 1) & ~(IntPtr.Size - 1);

        var ptr = ReserveMemory(size);

        // Write the header
        *ptr = 0;
        ptr++;

        // Write the MT
        *ptr = mt;

        var dataPtr = (byte*)(ptr + 1);

        // Write the length
        *(int*)dataPtr = data.Length;

        // Write the chars
        var destination = new Span<char>(dataPtr + sizeof(int), data.Length + 1);

        data.CopyTo(destination);
        destination[^1] = '\0';

        return *(string*)&ptr;
    }

    public T[] AllocateArray<T>(int length)
    {
        var arrayMt = typeof(T[]).TypeHandle.Value;
        var arrayMethodTable = *(MethodTable*)arrayMt;

        var arraySize = arrayMethodTable.BaseSize + length * arrayMethodTable.ComponentSize;

        var ptr = ReserveMemory(arraySize);

        // Write the header
        *ptr = 0;
        ptr++;

        // Write the MT
        *ptr = arrayMt;

        // Write the length
        *(ptr + 1) = length;

        return (T[])*(Array*)&ptr;
    }
}