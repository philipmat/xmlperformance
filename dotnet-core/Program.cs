﻿using System;
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
            var result = Task.Run(async () =>
            {
                return await MainAsync(args);
            }).GetAwaiter().GetResult();
            return result;
        }

        private static async Task<int> MainAsync(string[] args) {
            var file = args[0];
            var statsCollector = new StatsCollector();
            var parser = GetParser(file);
            if (parser == null) return ExitCodeIncorrectFile;

            var nodes = await parser.ParseAsync(statsCollector);
            await statsCollector.PrintStatsAsync(Console.Out);
            return 0;
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
