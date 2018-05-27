using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XmlPerformance
{
    interface IVisitor {
        void Visit(Label label);
        void Visit(Artist artist);
    }

    interface IAcceptVisitor<T>
    {
        void Accept(T visitor);
    }

    class StatsCollector : IVisitor
    {
        private readonly Dictionary<string, int> _dataQualityLedger = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
            ["Complete And Correct"] = 0,
            ["Correct"] = 0,
            ["Entirely Incorrect"] = 0,
            ["Entirely Incorrect Edit"] = 0,
            ["Needs Major Changes"] = 0,
            ["Needs Minor Changes"] = 0,
            ["Needs Vote"] = 0,
        };

        public void Visit(Label label) {
            if (label.Id == 0) return;
            if (string.IsNullOrEmpty(label.DataQuality)) {
                Console.Error.WriteLine($"Label {label.Name} is missing data quality.");
                return;
            }
            IncrementDataQuality(label.DataQuality);
        }

        public void Visit(Artist artist) {
            IncrementDataQuality(artist.DataQuality);
        }

        public void PrintStats(TextWriter writer) {
            var total = _dataQualityLedger.Sum(x => x.Value);
            var elements = _dataQualityLedger.OrderBy(x => x.Key)
                .Select(x => $"  {x.Key,-25} = {x.Value,10:n0}");
            writer.WriteLine($"Total: {total:n0} entries.");
            writer.WriteLine(string.Join("\n", elements));
        }

        private void IncrementDataQuality(string dataQuality) {
            if (string.IsNullOrWhiteSpace(dataQuality)) return;
            _dataQualityLedger[dataQuality] = _dataQualityLedger[dataQuality] + 1;
        }
    }
}
