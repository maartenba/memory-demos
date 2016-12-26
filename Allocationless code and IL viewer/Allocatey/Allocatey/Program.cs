using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Allocatey
{
    class Program
    {
        static void Main(string[] args)
        {
            //BoxingRing();
            //ParamsArray();
            //Scale(new [] { 1, 2, 3, 4, 5}, 1, 5, 1);

            //PrepareBeers();

            for (var i = 0; i < 10; i++)
            {
                BeerLoader.LoadBeers2();
                Console.ReadLine();
            }

            Console.ReadLine();

            var strings = new string[] { "x", "y"};
            foreach (var s in strings)
            {
                Task.Run(() => Console.WriteLine(s));
            }
        }

        private static void PrepareBeers()
        {
            var random = new Random();

            using (var reader = new StreamReader(File.OpenRead("C:\\users\\maart\\desktop\\Allocatey\\beers.txt")))
            using (var writer = new JsonTextWriter(new StreamWriter(File.OpenWrite("C:\\users\\maart\\desktop\\Allocatey\\beers.json"))))
            {
                writer.WriteStartArray();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSplit = line.Split('\t');

                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteValue(lineSplit[1]);
                    writer.WritePropertyName("brewery");
                    writer.WriteValue(lineSplit[3]);
                    writer.WritePropertyName("rating");
                    writer.WriteValue((double)random.Next(50, 100) / 100);
                    writer.WritePropertyName("votes");
                    writer.WriteValue(random.Next(239, 568000));
                    writer.WriteEndObject();

                    Console.WriteLine(lineSplit[1]);
                }

                writer.WriteEndArray();
            }
        }

        private static int[] Scale(int[] inputs, int low, int high, int c)
        {
            var results = from x in inputs
                          where (x >= low) && (x <= high)
                          select (x * c);

            return results.ToArray();
        }

        private static double AverageWithinBounds(int[] inputs, int min, int max)
        {
            var filtered = from x in inputs
                           where (x >= min) && (x <= max)
                           select x;

            return filtered.Average();
        }

        private static void BoxingRing()
        {
            int i = 123;

            // The following line boxes i.
            object o = i;
            int j = (int)o;

            // The following line boxes 42 and true
            Console.WriteLine(string.Concat("Answer", 42, true));
        }

        private static void ParamsArray()
        {
            // this allocates a new array...
            ParamsArrayImpl();
        }

        private static void ParamsArrayImpl(params string[] data)
        {
            foreach (var x in data)
            {
                Console.WriteLine(x);
            }
        }
    }
}
