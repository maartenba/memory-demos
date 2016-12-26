namespace ClrMd.Explorer
{
    public class ClrHeapObject
    {
        public int Generation { get; }
        public ulong Ptr { get; }

        public ClrHeapObject(int generation, ulong ptr)
        {
            Generation = generation;
            Ptr = ptr;
        }
    }
}