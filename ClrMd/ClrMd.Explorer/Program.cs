using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using ClrMd.Explorer.GeekOut;
using Microsoft.Diagnostics.Runtime;

namespace ClrMd.Explorer;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
class Program
{
    static void Main(string[] args)
    {
        // Start the ClrMd.Target process
        var demoProcess = StartDemoProcess();

        // Give it a few seconds to run
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // Attach ClrMd to our process
        using (var dataTarget = DataTarget.AttachToProcess(demoProcess.Id, true)) // suspend, we want to explore the current state of the app
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
            if (runtime.Heap.CanWalkHeap)
            {
                DumpHeapObjects(dataTarget, clrVersion, runtime, appDomain, runtime.Heap);
                Console.ReadLine();

                DumpStringDuplicates(dataTarget, clrVersion, runtime, appDomain, runtime.Heap, top: 100);
                Console.ReadLine();

                DumpRetention(dataTarget, clrVersion, runtime, appDomain, runtime.Heap, "ClrMd.Target.Clock");
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
            .Replace("ClrMd.Explorer", "ClrMd.Target")
            .Replace(".dll", ".exe"));

        return Process.Start(processStartInfo);
    }

    private static void DumpClrInfo(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain)
    {
        // Dump CLR info
        Console.WriteLine("# CLR Info");
        Console.WriteLine("Version:        {0}", clrVersion.Version);
        Console.WriteLine("Flavor:         {0}", clrVersion.Flavor);
        Console.WriteLine("Size:           {0:X}", clrVersion.IndexFileSize);
        Console.WriteLine("Timestamp:      {0:X}", clrVersion.IndexTimeStamp);
        Console.WriteLine("");
    }

    private static void DumpRuntimeInfo(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain)
    {
        // Dump runtime info
        Console.WriteLine("# Runtime Info");
        Console.WriteLine("AppDomain:      {0}", appDomain.Name);
        Console.WriteLine("Address:        {0}", appDomain.Address);
        Console.WriteLine("");
    }

    private static void DumpThreadInfo(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain)
    {
        // Dump thread info
        Console.WriteLine("## Threads");
        Console.WriteLine("Thread count:   {0}", runtime.Threads.Length);
        Console.WriteLine("");
        foreach (var thread in runtime.Threads)
        {
            Console.WriteLine("### Thread {0}", thread.OSThreadId);
            Console.WriteLine("Thread type: {0}", thread.IsGc
                ? "GC"
                : thread.IsFinalizer
                    ? "Finalizer"
                    : "");

            Console.WriteLine("");
            Console.WriteLine("Stack trace:");
            foreach (var stackFrame in thread.EnumerateStackTrace())
            {
                Console.WriteLine("* {0}", stackFrame);
            }
            Console.WriteLine("");
        }
    }

    private static void DumpHeapObjects(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain, ClrHeap heap)
    {
        Console.WriteLine("## Heap objects");
        
        foreach (var generation in heap
                     .EnumerateObjects()
                     .GroupBy(obj => heap.GetSegmentByAddress(obj.Address)?.Kind))
        {
            Console.WriteLine("### {0}", generation.Key?.ToString() ?? "Unknown");
            foreach (var clrObject in generation)
            {
                var type = clrObject.Type;
                if (type == null)
                {
                    continue;
                }

                Console.WriteLine("* {0} - {1} - {2} bytes", clrObject.Address, type.Name, clrObject.Size);
                if (type.IsString)
                {
                    Console.WriteLine("** Value: {0}", clrObject.AsString());
                }
            }
            Console.WriteLine("");
        }
    }

    private static void DumpStringDuplicates(DataTarget dataTarget, ClrInfo clrVersion, ClrRuntime runtime, ClrAppDomain appDomain, ClrHeap heap, int top = 10)
    {
        var numberOfStrings = 0;
        var uniqueStrings = new Dictionary<string, int>();

        foreach (var clrObject in heap.EnumerateObjects())
        {
            var type = clrObject.Type;

            // Skip if not a string
            if (type == null || type.IsString == false)
            {
                continue;
            }

            // Count total
            numberOfStrings++;

            // Get value
            var text = clrObject.AsString();
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
        var graph = new Dgml();

        Console.WriteLine("## What's the retention path of the {0} object?", targetType);
        Console.WriteLine("");
        foreach (var clrObject in heap.EnumerateObjects())
        {
            var type = heap.GetObjectType(clrObject);
            if (type == null || type.Name != targetType)
            {
                continue;
            }

            // Enumerate roots and try to find the current object
            var stack = new Stack<ClrObject>();
            foreach (var root in heap.EnumerateRoots())
            {
                stack.Clear();
                stack.Push(root.Object);
                if (GetPathToObject(heap, clrObject, stack, new HashSet<ClrObject>()))
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

                        Console.WriteLine("{0} {1} - {2} - {3} bytes", new string('+', depth++), address, t.Name, clrObject.Size);

                        graph.AddNode(address.ToString(), $"{t.Name} ({address})");
                        if (previousAddress > 0 && !graph.Links.Any(e => e.Source == previousAddress.ToString() && e.Target == address.ToString()))
                        {
                            graph.AddLink(previousAddress.ToString(), address.ToString());
                        }
                        previousAddress = address;
                    }

                    Console.WriteLine();
                }
            }
        }

        Console.ReadLine();
        
        // Write graph
        using var writer = new XmlTextWriter(new StreamWriter(File.OpenWrite("file.dgml")));
        graph.WriteTo(writer);
        writer.Close();
    }

    private static bool GetPathToObject(ClrHeap heap, ClrObject clrObject, Stack<ClrObject> stack, HashSet<ClrObject> touchedObjects)
    {
        // Start of the journey - get address of the first objetc on our reference chain
        var currentObject = stack.Peek();

        // Have we checked this object before?
        if (!touchedObjects.Add(currentObject))
        {
            return false;
        }

        // Did we find our object? Then we have the path!
        if (currentObject == clrObject)
        {
            return true;
        }

        // Enumerate internal references of the object
        var found = false;
        var type = heap.GetObjectType(currentObject);
        if (type != null)
        {
            foreach (var innerObject in currentObject.EnumerateReferences())
            {
                if (innerObject.Address == 0 || touchedObjects.Contains(innerObject)) break;
                
                // Push the object onto our stack
                stack.Push(innerObject);
                if (GetPathToObject(heap, clrObject, stack, touchedObjects))
                {
                    found = true;
                    break;
                }
            }
        }

        return found;
    }
}