using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Allocatey
{
    class Demo02_ValidateArgumentsDemo
    {
        void SomeMethod(string directory)
        {
            Ensure.IsTrue1(Directory.Exists(directory), 
                $"The directory '{directory}' does not exist.", // <-- allocation
                nameof(directory));

            Ensure.IsTrue2(Directory.Exists(directory),
                () => $"The directory '{directory}' does not exist.", // <-- allocation! capturing state... 2 objects instead of above one
                nameof(directory));

            Ensure.IsTrue3(Directory.Exists(directory),
                nameof(directory),
                "The directory '{0}' does not exist.",
                directory); // Array allocated...

            Ensure.IsTrue4(Directory.Exists(directory),
                nameof(directory),
                "The directory '{0}' does not exist.",
                directory); // No allocations!

            // ...
        }
    }

    public class Ensure
    {
        public static void IsTrue1(bool condition, string exceptionMessage, string parameterName)
        {
            if (!condition)
            {
                throw new ArgumentException(exceptionMessage, parameterName);
            }
        }

        public static void IsTrue2(bool condition, Func<string> exceptionMessage, string parameterName)
        {
            if (!condition)
            {
                throw new ArgumentException(exceptionMessage(), parameterName);
            }
        }

        public static void IsTrue3(bool condition, string parameterName, string exceptionMessage, params string[] args)
        {
            if (!condition)
            {
                throw new ArgumentException(string.Format(exceptionMessage, args), parameterName);
            }
        }
        
        public static void IsTrue4(bool condition, string parameterName, string exceptionMessage, string arg0)
        {
            if (!condition)
            {
                throw new ArgumentException(string.Format(exceptionMessage, arg0), parameterName);
            }
        }
        
        public static void IsTrue4(bool condition, string parameterName, string exceptionMessage, string arg0, string arg1)
        {
            if (!condition)
            {
                throw new ArgumentException(string.Format(exceptionMessage, arg0, arg1), parameterName);
            }
        }
    }
}