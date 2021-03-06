﻿#undef ASYNC
using System;
using System.Threading.Tasks;

namespace XmlPerformance
{
    class Program
    {
        private const int ExitCodeIncorrectParams = 1;
        private const int ExitCodeIncorrectFile = 2;

        public static int Main(string[] args) {
            if(args.Length != 1) {
                Help();
                return ExitCodeIncorrectParams;
            }
            var file = args[0];
            var statsCollector = new StatsCollector();
            var parser = GetParser(file);
            if (parser == null) return ExitCodeIncorrectFile;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            int nodes = 0;
#if ASYNC
            nodes = Task.Run(async () =>
            {
                return await parser.ParseAsync(statsCollector);
            }).GetAwaiter().GetResult();
#else
            nodes = parser.Parse(statsCollector);
#endif

            // get measurements before performing any operations in collector
            sw.Stop();
            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            long totalBytesOfMemoryUsed = currentProcess.WorkingSet64; 

            // print stats
            statsCollector.PrintStats(Console.Out);
            Console.Out.WriteLine($"Parsing of {file} took {FormatTime(sw.ElapsedMilliseconds)} and used {totalBytesOfMemoryUsed:n0} bytes of memory.");
            return 0;
        }

        private static string FormatTime(long milliseconds) {
            float multiplier = 1;
            string unit = "ms";
            if (milliseconds > 10_000) {
                multiplier = 1E-3F;
                unit = "s";
            }
            return $"{milliseconds*multiplier:n0}{unit}";
        }

        private static IParser GetParser(string file) {
            if (file.IndexOf("labels", StringComparison.OrdinalIgnoreCase) != -1) {
                Console.WriteLine("Performing Labels test");
                return new LabelParser(file);
            }
            if (file.IndexOf("artists", StringComparison.OrdinalIgnoreCase) != -1) {
                Console.WriteLine("Performing Artists test");
                return new ArtistParser(file);
            }
            return null;
        }

        private static void Help() {
            Console.WriteLine(@"
This program takes only one parameter: the path to the Discogs XML dump.

If the file contains ""labels"", the labels test will be performed.
If the file contains ""artists"", the artists test will be performed.
");
        } 
    }
}
