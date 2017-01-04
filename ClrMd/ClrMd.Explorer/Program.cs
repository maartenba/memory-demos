using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using ClrMd.Explorer.GeekOut;
using Microsoft.Diagnostics.Runtime;

namespace ClrMd.Explorer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the ClrMd.Target process
            var demoProcess = StartDemoProcess();

            // Give it a few seconds to run
            Thread.Sleep(TimeSpan.FromSeconds(2));

            // Attach ClrMd to our process
            using (var dataTarget = DataTarget.AttachToProcess(demoProcess.Id, 10000, AttachFlag.Invasive)) // invasive, pausing our target process
            {
                // Get CLR version, runtime and appdomain
                var clrVersion = dataTarget.ClrVersions.First();
                var runtime = clrVersion.CreateRuntime();
                var appDomain = runtime.AppDomains.First();

                // Dump some info about the process
                DumpClrInfo(dataTarget, clrVersion, runtime, appDomain);
                Console.ReadLine();

                DumpRuntimeInfo(dataTarget, clrVersion, runtime, appDomain);
                Console.ReadLine();

                DumpThreadInfo(dataTarget, clrVersion, runtime, appDomain);
                Console.ReadLine();

                // Get heap
                var heap = runtime.GetHeap();
                if (heap.CanWalkHeap)
                {
                    DumpHeapObjects(dataTarget, clrVersion, runtime, appDomain, heap);
                    Console.ReadLine();

                    DumpStringDuplicates(dataTarget, clrVersion, runtime, appDomain, heap, top: 100);
                    Console.ReadLine();

                    DumpRetention(dataTarget, clrVersion, runtime, appDomain, heap, "ClrMd.Target.Clock");
                    Console.ReadLine();
                }
            }

            // Wait
            Console.WriteLine("Press <enter> to quit");
            Console.ReadLine();

            // Kill demo process
            try
            {
                demoProcess.Kill();
            }
            catch
            {
            }
        }

        private static Process StartDemoProcess()
        {
            // Start the ClrMd.Target process
            var processStartInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location
                .Replace("ClrMd.Explorer", "ClrMd.Target"));

            return Process.Start(processStartInfo);
        }

        private static void DumpClrInfo(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain)
        {
            // Dump CLR info
            var dacInfo = clrVersion.DacInfo;

            Console.WriteLine("# CLR Info");
            Console.WriteLine("Version:   {0}", clrVersion.Version);
            Console.WriteLine("Filesize:  {0:X}", dacInfo.FileSize);
            Console.WriteLine("Timestamp: {0:X}", dacInfo.TimeStamp);
            Console.WriteLine("Dac file:  {0}", dacInfo.FileName);
            Console.WriteLine("");
        }

        private static void DumpRuntimeInfo(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain)
        {
            // Dump runtime info
            Console.WriteLine("# Runtime Info");
            Console.WriteLine("AppDomain:      {0}", appDomain.Name);
            Console.WriteLine("Address:        {0}", appDomain.Address);
            Console.WriteLine("Configuration:  {0}", appDomain.ConfigurationFile);
            Console.WriteLine("Directory:      {0}", appDomain.ApplicationBase);
            Console.WriteLine("");
        }

        private static void DumpThreadInfo(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain)
        {
            // Dump thread info
            Console.WriteLine("## Threads");
            Console.WriteLine("Thread count:   {0}", runtime.Threads.Count);
            Console.WriteLine("");
            foreach (var thread in runtime.Threads)
            {
                Console.WriteLine("### Thread {0}", thread.OSThreadId);
                Console.WriteLine("Thread type: {0}", thread.IsBackground
                    ? "Background"
                    : thread.IsGC
                        ? "GC"
                        : "Foreground");

                Console.WriteLine("");
                Console.WriteLine("Stack trace:");
                foreach (var stackFrame in thread.EnumerateStackTrace())
                {
                    Console.WriteLine("* {0}", stackFrame.DisplayString);
                }
                Console.WriteLine("");
            }
        }

        private static void DumpHeapObjects(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain, ClrHeap heap)
        {
            Console.WriteLine("## Heap objects");
            Console.WriteLine("Heap count:     {0}", runtime.HeapCount);
            Console.WriteLine("");

            foreach (var generation in heap.EnumerateObjectAddresses()
                .Select(ptr => new ClrHeapObject(heap.GetGeneration(ptr), ptr))
                .GroupBy(item => item.Generation)
                .OrderBy(item => item.Key))
            {
                Console.WriteLine("### Generation {0}", generation.Key);
                foreach (var objectAddress in generation)
                {
                    var type = heap.GetObjectType(objectAddress.Ptr);
                    if (type == null)
                    {
                        continue;
                    }

                    Console.WriteLine("* {0} - {1} - {2} bytes", objectAddress.Ptr, type.Name, type.GetSize(objectAddress.Ptr));
                    if (type.HasSimpleValue)
                    {
                        Console.WriteLine("** Value: {0}", type.GetValue(objectAddress.Ptr));
                    }
                }
                Console.WriteLine("");
            }
        }

        private static void DumpStringDuplicates(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain, ClrHeap heap, int top = 10)
        {
            var numberOfStrings = 0;
            var uniqueStrings = new Dictionary<string, int>();

            foreach (var ptr in heap.EnumerateObjectAddresses())
            {
                var type = heap.GetObjectType(ptr);

                // Skip if not a string
                if (type == null || type.IsString == false)
                {
                    continue;
                }

                // Count total
                numberOfStrings++;

                // Get value
                var text = (string)type.GetValue(ptr);
                if (uniqueStrings.ContainsKey(text))
                {
                    uniqueStrings[text]++;
                }
                else
                {
                    uniqueStrings[text] = 1;
                }
            }

            Console.WriteLine("## String info");
            Console.WriteLine("String count:     {0}", numberOfStrings);
            Console.WriteLine("");

            Console.WriteLine("Most duplicated strings: (top {0})", top);
            foreach (var keyValuePair in uniqueStrings.OrderByDescending(kvp => kvp.Value).Take(top))
            {
                Console.WriteLine("* {0} usages: {1}", keyValuePair.Value, keyValuePair.Key);
            }
            Console.WriteLine("");
        }

        private static void DumpRetention(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain, ClrHeap heap, string targetType)
        {
            var dgml = new Dgml();

            Console.WriteLine("## What's the retention path of the {0} object?", targetType);
            Console.WriteLine("");
            foreach (var ptr in heap.EnumerateObjectAddresses())
            {
                var type = heap.GetObjectType(ptr);
                if (type == null || type.Name != targetType)
                {
                    continue;
                }

                // Enumerate roots and try to find the current object
                var stack = new Stack<ulong>();
                foreach (var root in heap.EnumerateRoots())
                {
                    stack.Clear();
                    stack.Push(root.Object);
                    if (GetPathToObject(heap, ptr, stack, new HashSet<ulong>()))
                    {
                        // Print retention path
                        var depth = 0;
                        var previousAddress = (ulong)0;
                        foreach (var address in stack)
                        {
                            var t = heap.GetObjectType(address);
                            if (t == null)
                            {
                                continue;
                            }

                            Console.WriteLine("{0} {1} - {2} - {3} bytes", new string('+', depth++), address, t.Name, t.GetSize(address));

                            dgml.AddNode(address.ToString(), string.Format("{0} ({1})", t.Name, address));
                            if (previousAddress > 0)
                            {
                                dgml.AddLink(previousAddress.ToString(), address.ToString());
                            }
                            previousAddress = address;
                        }

                        Console.WriteLine();
                    }
                }
            }

            using (var writer = new XmlTextWriter("retention.dgml", Encoding.UTF8))
            {
                dgml.WriteTo(writer);
            }

            Console.ReadLine();
            Process.Start("retention.dgml");
        }

        private static bool GetPathToObject(ClrHeap heap, ulong objectPointer, Stack<ulong> stack, HashSet<ulong> touchedObjects)
        {
            // Start of the journey - get address of the first objetc on our reference chain
            var currentObject = stack.Peek();

            // Have we checked this object before?
            if (!touchedObjects.Add(currentObject))
            {
                return false;
            }

            // Did we find our object? Then we have the path!
            if (currentObject == objectPointer)
            {
                return true;
            }


            // Enumerate internal references of the object
            var found = false;
            var type = heap.GetObjectType(currentObject);
            if (type != null)
            {
                type.EnumerateRefsOfObject(currentObject, (innerObject, fieldOffset) =>
                {
                    if (innerObject == 0 || touchedObjects.Contains(innerObject))
                    {
                        return;
                    }

                    // Push the object onto our stack
                    stack.Push(innerObject);
                    if (GetPathToObject(heap, objectPointer, stack, touchedObjects))
                    {
                        found = true;
                        return;
                    }

                    // If not found, pop the object from our stack as this is not the tree we're looking for
                    stack.Pop();
                });
            }

            return found;
        }
    }
}
