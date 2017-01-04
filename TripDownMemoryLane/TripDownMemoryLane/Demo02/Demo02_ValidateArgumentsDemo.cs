using System;
using System.IO;

namespace TripDownMemoryLane.Demo02
{
    class Demo02_ValidateArgumentsDemo
    {
        void Attempt1(string directory1)
        {
            // allocates a string and runs string.Format even if condition is okay
            Ensure.IsTrue1(Directory.Exists(directory1),
                $"The directory '{directory1}' does not exist.",
                nameof(directory1));
        }

        void Attempt2(string directory2)
        {
            // does not allocate a string! but captures state, making it two allocations...
            // (func and state object)
            Ensure.IsTrue2(Directory.Exists(directory2),
                () => $"The directory '{directory2}' does not exist.",
                nameof(directory2));
        }

        void Attempt3(string directory3)
        {
            // allocates an array
            Ensure.IsTrue3(Directory.Exists(directory3),
                nameof(directory3),
                "The directory '{0}' does not exist.",
                directory3);
        }

        void Attempt4(string directory4)
        {
            // no allocations!
            Ensure.IsTrue4(Directory.Exists(directory4),
                nameof(directory4),
                "The directory '{0}' does not exist.",
                directory4);
        }
    }

    public class Ensure
    {
        public static void IsTrue1(bool condition, string exceptionMessage, string parameterName)
        {
            if (!condition)
            {
                throw new ArgumentException(
                    exceptionMessage, parameterName);
            }
        }

        public static void IsTrue2(bool condition, Func<string> exceptionMessage, string parameterName)
        {
            if (!condition)
            {
                throw new ArgumentException(
                    exceptionMessage(), parameterName);
            }
        }

        public static void IsTrue3(bool condition, string parameterName, string exceptionMessage, params string[] args)
        {
            if (!condition)
            {
                throw new ArgumentException(
                    string.Format(exceptionMessage, args), parameterName);
            }
        }
        
        public static void IsTrue4(bool condition, string parameterName, string exceptionMessage, string arg0)
        {
            if (!condition)
            {
                throw new ArgumentException(
                    string.Format(exceptionMessage, arg0), parameterName);
            }
        }
        
        public static void IsTrue4(bool condition, string parameterName, string exceptionMessage, string arg0, string arg1)
        {
            if (!condition)
            {
                throw new ArgumentException(
                    string.Format(exceptionMessage, arg0, arg1), parameterName);
            }
        }
    }
}