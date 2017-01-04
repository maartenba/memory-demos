using System;
using System.Collections.Generic;

namespace TripDownMemoryLane.Demo01
{
    public class Cache
    {
        // Dictionary to contain the cache.
        private static Dictionary<int, WeakReference<Data>> _cache;

        // Track the number of times an object is regenerated.
        private int regenCount = 0;

        public Cache(int count)
        {
            _cache = new Dictionary<int, WeakReference<Data>>();

            // Add objects with a short weak reference to the cache.
            for (int i = 0; i < count; i++)
            {
                _cache.Add(i, new WeakReference<Data>(new Data(i), false));
            }
        }

        // Number of items in the cache.
        public int Count
        {
            get { return _cache.Count; }
        }

        // Number of times an object needs to be regenerated.
        public int RegenerationCount
        {
            get { return regenCount; }
        }

        // Retrieve a data object from the cache.
        public Data this[int index]
        {
            get
            {
                Data d;
                if (!_cache[index].TryGetTarget(out d))
                {
                    // If the object was reclaimed, generate a new one.
                    Console.WriteLine("Regenerate object at {0}: Yes", index);
                    d = new Data(index);
                    _cache[index] = new WeakReference<Data>(d, false);
                    regenCount++;
                }
                else
                {
                    // Object was obtained with the weak reference.
                    Console.WriteLine("Regenerate object at {0}: No", index);
                }

                return d;
            }
        }
    }
}