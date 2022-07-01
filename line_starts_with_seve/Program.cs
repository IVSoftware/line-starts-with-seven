using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace line_starts_with_seve
{
    class Program
    {
        static void Main(string[] args)
        {
            int totalCount;
            TimeSpan original, optimizedOriginal, linq;
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "TextFile.txt");
            try
            {
                // 200K lines of random guids
                List<string> builder = 
                    Enumerable.Range(0, 200000)
                    // To be on the safe side, make sure a curly brace is the first char
                    .Select(n => $"{{{System.Guid.NewGuid().ToString()}}}")
                    .ToList();

                // This loads the original lines from the post...
                var footer =
                    File.ReadAllLines(path);
                // ... and appends it to the 200K lines
                builder.AddRange(footer);

                // Now we have an array we can use for the test
                var FileLines = builder.ToArray();

                var benchmark = new System.Diagnostics.Stopwatch();

                // This is the substring error was caught. The line starting with '7'
                // wan't being hit. The expected return value of 4 wasn't received.
                totalCount = int.MinValue;

                benchmark.Start();
                foreach (var line in FileLines)
                {
                    //// If line length is zero, then do nothing
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    // Original code from post
                    // switch (line.Substring(1, 1))
                    // Should be:
                    switch (line.Substring(0, 1))
                    {
                        case "7":
                            totalCount = int.Parse(line.Substring(4, 6));
                            // This is another issue!! Breaking from the switch DOESN'T break from the loop
                            break;
                            // SHOULD BE: goto breakFromInner;
                            // One of the few good reasons to use a goto statement!!
                    }
                }
                // See note
                // breakFromInner:
                benchmark.Stop();
                original = benchmark.Elapsed;
                Debug.Assert(totalCount == 4);
                Console.WriteLine($"200K lines using Original code: Elapsed = {benchmark.Elapsed}");
                Console.WriteLine($"Count = {totalCount}");


                totalCount = int.MinValue;
                benchmark.Restart();
                for (int i = FileLines.Length - 1; i >= 0; i--)
                {
                    var line = FileLines[i];
                    //// If line length is zero, then do nothing
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    // Original code from post
                    // switch (line.Substring(1, 1))
                    // Should be:
                    switch (line.Substring(0, 1))
                    {
                        case "7":
                            totalCount = int.Parse(line.Substring(4, 6));
                            // One of the few good reasons to use a goto statement!!
                            goto breakFromInner;
                    }
                }
            // See note
            breakFromInner:
                benchmark.Stop();
                optimizedOriginal = benchmark.Elapsed;
                Debug.Assert(totalCount == 4);
                Console.WriteLine($"200K lines using Original code with reverse: Elapsed = {benchmark.Elapsed}");
                Console.WriteLine($"Count = {totalCount}");

                // Expecting an iterator here, not an array.
                Console.WriteLine($"Return type of Reverse is: '{FileLines.Reverse()}'");

                benchmark.Restart();
                totalCount =
                    int.Parse(
                        FileLines
                        .Reverse()
                        .First(line => line.Any() && (line.First() == '7'))
                        .Substring(4, 6));
                benchmark.Stop();
                linq = benchmark.Elapsed;
                Debug.Assert(totalCount == 4);
                Console.WriteLine($"200K lines using Linq with Reverse: Elapsed = {benchmark.Elapsed}");
                Console.WriteLine($"Count = {totalCount}{Environment.NewLine}");

                Console.WriteLine("PRELIMINARY RESULTS");
                Console.WriteLine($"Linq runs ~{original.Ticks / linq.Ticks} faster than Original");
                Console.WriteLine($"OptimizedOriginal runs ~{linq.Ticks / optimizedOriginal.Ticks} faster than Linq");
                Console.WriteLine($"OptimizedOriginal runs ~{original.Ticks / optimizedOriginal.Ticks} faster than Original");
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }
    }
}
