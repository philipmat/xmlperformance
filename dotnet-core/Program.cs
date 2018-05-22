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
            if (file.IndexOf("labels", StringComparison.OrdinalIgnoreCase) != -1) {
                Console.WriteLine("Performing Labels test");
                Task.Run(async () =>
                {
                    var nodes = await new LabelParser(file).ParseAsync(statsCollector);
                    await statsCollector.PrintStatsAsync(Console.Out);
                }).GetAwaiter().GetResult();
                return 0;
            }
            if (file.IndexOf("artists", StringComparison.OrdinalIgnoreCase) != -1) {
                Console.WriteLine("Performing Artists test");
                return 0;
            }

            return ExitCodeIncorrectFile;
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
