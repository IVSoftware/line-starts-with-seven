using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace line_starts_with_seve
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "TextFile.txt");
            try
            {
                // 200K lines of random guids
                List<string> builder = 
                    Enumerable.Range(0, 200000)
                    .Select(n => new System.Guid().ToString())
                    .ToList();

                var footer =
                    File.ReadAllLines(path);

                builder.AddRange(footer);

                var FileLines = builder.ToArray();

                var benchmark = new System.Diagnostics.Stopwatch();
                benchmark.Start();
                int totalCount;
                foreach (var line in FileLines)
                {
                    //// If line length is zero, then do nothing
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    switch (line.Substring(1, 1))
                    {
                        case "7":
                            totalCount = int.Parse(line.Substring(4, 6));
                            // This is another issue!! Breaking from the switch DOESN'T break from the loop
                            break;
                            // SHOULD BE: goto breakFromInner;
                            // One of the few good reasons to use a goto statement!!
                    }
                }
                benchmark.Stop();
                Console.WriteLine($"200K lines using Original code: Elapsed = {benchmark.Elapsed}");


                benchmark.Restart();
                for (int i = FileLines.Length - 1; i >= 0; i--)
                {
                    var line = FileLines[i];
                    //// If line length is zero, then do nothing
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    switch (line.Substring(1, 1))
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
                Console.WriteLine($"200K lines using Original code with reverse: Elapsed = {benchmark.Elapsed}");

                benchmark.Restart();
                var count =
                    int.Parse(
                        FileLines
                        .Reverse()
                        .First(line => line.Any() && (line.First() == '7'))
                        .Substring(4, 6));
                benchmark.Stop();
                Console.WriteLine($"200K lines using Linq with Reverse: Elapsed = {benchmark.Elapsed}");
                Console.WriteLine($"Count = {count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Assert(false, ex.Message);
            }
        }
    }
}
