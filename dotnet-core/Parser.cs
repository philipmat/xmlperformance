using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace XmlPerformance
{
    abstract class Parser<T>
        where T : IAcceptVisitor<IVisitor>, new()
    {
        private readonly string _file;

        public Parser(string file)
        {
            this._file = file;
        }
        public async Task<int> ParseAsync(IVisitor statsCollector)
        {
            var nodes = 0;
            var readerSettings = new XmlReaderSettings {
                Async = true
            };
            using(var reader = XmlReader.Create(_file, readerSettings)) {
                reader.ReadToFollowing(MainNodeName);
                T current = new T();
                while(await reader.ReadAsync()) {
                    Console.WriteLine($"{reader.Name} - {reader.NodeType}");
                    if (reader.NodeType == XmlNodeType.Element) {
                        // Console.WriteLine($"{reader.Name} [{reader.NodeType}]");
                        var nodeName = reader.Name;
                        if (SkipNodes.Any(textNode => textNode.Equals(nodeName, StringComparison.OrdinalIgnoreCase))) {
                            reader.Skip();
                            continue;
                        }
                        if (nodeName.Equals(MainNodeName, StringComparison.OrdinalIgnoreCase)) {
                            // finish last element
                            current?.Accept(statsCollector);
                            current = new T();
                        }
                        if (TextNodes.Any(textNode => textNode.Equals(nodeName, StringComparison.OrdinalIgnoreCase))) {
                            var text = await reader.ReadElementContentAsStringAsync();
                            SetNodeText(current, nodeName, text);
                        }
                        nodes++;
                    }
                }
            }
            return nodes;
        }

        protected abstract string MainNodeName { get; }

        /// <summary>Comma-
        protected abstract string[] TextNodes { get; } //
        protected abstract string[] SkipNodes { get; } //

        protected abstract void SetNodeText(T currentElement, string nodeName, string text);
    }
}
