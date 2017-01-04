using System;
using System.Linq;
using System.Threading.Tasks;

namespace Allocatey.Talk
{
    class Demo02_Random
    {
        // 1.
        private static void BoxingRing()
        {
            int i = 123;

            // The following line boxes i.
            object o = i;

            // This one unboxes
            int j = (int)o;

            // The following line boxes 42 and true
            Console.WriteLine(string.Concat("Answer", 42, true));
        }

        // 2.
        private static void ParamsArray()
        {
            ParamsArrayImpl();
        }

        private static void ParamsArrayImpl(params string[] data)
        {
            foreach (var x in data)
            {
                Console.WriteLine(x);
            }
        }

        // 3.
        private static double AverageWithinBounds(int[] inputs, int min, int max)
        {
            var filtered = from x in inputs
                           where (x >= min) && (x <= max)
                           select x;

            return filtered.Average();
        }

        // 4. 
        private static void Lambdas()
        {
            var strings = new string[] { "x", "y" };
            foreach (var s in strings)
            {
                Task.Run(() => Console.WriteLine(s));
            }
        }
    }
}
