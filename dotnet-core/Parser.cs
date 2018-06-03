using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace XmlPerformance
{
    interface IParser
    {
#if ASYNC
        Task<int> ParseAsync(IVisitor statsCollector);
#else
        int Parse(IVisitor statsCollector);
#endif
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

#if ASYNC
        public async Task<int> ParseAsync(IVisitor statsCollector) {
#else
        public int Parse(IVisitor statsCollector) {
#endif
            var nodes = 0;
            var readerSettings = new XmlReaderSettings {
                Async = true
            };
            InputStream inputStream = GetInputStream();
            try {
                using (var reader = XmlReader.Create(inputStream.ReadStream, readerSettings)) {
                    reader.ReadToFollowing(MainNodeName);
                    T current = new T();
                    string lastElement = null;
#if ASYNC
                    while (await reader.ReadAsync())
#else
                    while(reader.Read())
#endif
                    {
                        Read(reader, ref current, ref lastElement, statsCollector);
                        nodes++;
                    }
                    if (current != null) {
                        CollectStats(statsCollector, current);
                    }
                }
            } finally {
                inputStream.Dispose();
            }
            return nodes;
        }

        private void Read(XmlReader reader, ref T current, ref string lastElement,  IVisitor statsCollector) {
            if (reader.NodeType == XmlNodeType.Element && reader.IsStartElement()) {
                if (ShouldSkip(reader)) {
                    // skips the current node and advances to the next one
                    reader.Skip();
                    if (reader.NodeType == XmlNodeType.EndElement) {
                        // next node is an end-element, continue
                        return;
                    }
                }
                var nodeName = reader.Name;
                if (IsNewMainNode(nodeName)) {
                    // collect stats and start new node
                    CollectStats(statsCollector, current);
                    current = new T();
                    return;
                }
                lastElement = IsRelevantNode(nodeName) ? nodeName : null;
            } else if (IsTextNode(reader, lastElement)) {
                SetNodeText(current, lastElement, reader.Value);
            }
        }

        private InputStream GetInputStream() {
            return new InputStream(_file);
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

        private class InputStream : IDisposable
        {
            private bool disposedValue = false; // To detect redundant calls
            private Stream fileStream;
            private Stream readStream;

            public InputStream(string file)
            {
                readStream = fileStream = File.OpenRead(file);;
                if(file.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)) {
                    readStream = new GZipStream(fileStream, CompressionMode.Decompress);
                }
            }

            public Stream ReadStream => readStream;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                        readStream.Dispose();
                        fileStream.Dispose();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
            }
        }
    }
}
