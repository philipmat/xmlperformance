using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace XmlPerformance
{
    interface IParser
    {
        Task<int> ParseAsync(IVisitor statsCollector);
    }

    /// <summary>
    /// Root class of individual parsers.
    /// Assumes a simple &lt;tag&gt;text&lt;/tag&gt; approach to parsing, with no deep levels
    /// </summary>
    abstract class Parser<T> : IParser
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
                string lastElement = null;
                while(await reader.ReadAsync()) {
                    if (reader.NodeType == XmlNodeType.Element && reader.IsStartElement()) {
                        if(ShouldSkip(reader))
                        {
                            // skips the current node and advances to the next one
                            reader.Skip();
                            if (reader.NodeType == XmlNodeType.EndElement) {
                                // next node is an end-element, continue
                                continue;
                            }
                        }
                        var nodeName = reader.Name;
                        if (IsNewMainNode(nodeName)) {
                            // collect stats and start new node
                            CollectStats(statsCollector, current);
                            current = new T();
                            continue;
                        }
                        lastElement = IsRelevantNode(nodeName) ? nodeName : null;
                        nodes++;
                    } else if (IsTextNode(reader, lastElement)) {
                        SetNodeText(current, lastElement, reader.Value);
                    }
                }
                if (current != null) {
                    CollectStats(statsCollector, current);
                }
            }
            return nodes;
        }

        private void CollectStats(IVisitor statsCollector, T current)
        {
            current?.Accept(statsCollector);
        }

        protected abstract string MainNodeName { get; }

        /// <summary>Nodes whose text we care to collect (eg id, name)</summary>
        protected abstract string[] TextNodes { get; } //

        /// <summary>Nodes whose children we can skip parsing (images, sublabels)</summary>
        protected abstract string[] SkipNodes { get; } //
        
        protected abstract IDictionary<string, Action<T, string>> ElementSetters { get; }


        protected virtual void SetNodeText(T current, string nodeName, string text) {
#if (VERBOSE)
            if (nodeName == "id")
                System.Console.WriteLine($"[{nodeName, -20}] = {text}");
#endif
            if (current == null) return;
            Action<T, string> setter;
            if (ElementSetters.TryGetValue(nodeName, out setter)) {
                setter(current, text);
            }
        }

        private static bool IsTextNode(XmlReader reader, string lastElement)
        {
            return reader.NodeType == XmlNodeType.Text && !string.IsNullOrEmpty(lastElement);
        }

        /// <summary>Skips the current node and advances to the next one if it's part of <see cref="SkipNodes"/>.null</summary>
        private bool ShouldSkip(XmlReader reader) {
            return SkipNodes.Any(textNode => textNode.Equals(reader.Name, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsNewMainNode(string nodeName) {
            return nodeName.Equals(MainNodeName, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsRelevantNode(string nodeName)
        {
            return TextNodes.Any(textNode => textNode.Equals(nodeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
