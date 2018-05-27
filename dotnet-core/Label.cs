using System;
using System.Collections.Generic;

namespace XmlPerformance
{
    [System.Diagnostics.DebuggerDisplay("{Id} - {Name} ({DataQuality})")]
    class Label : IAcceptVisitor<IVisitor>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public string Profile { get; set; }
        public string DataQuality { get; set; } // TODO: enum?
        public ICollection<string> Urls { get; } = new List<string>();
        public ICollection<int> Sublabels { get; } = new List<int>();
        public int? ParentLabel { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    class LabelParser : Parser<Label>
    {
        private readonly Dictionary<string, Action<Label, string>> _setters = new Dictionary<string, Action<Label, string>>(StringComparer.OrdinalIgnoreCase) {
            ["id"] = (l, v) => l.Id = int.Parse(v),
            ["name"] = (l, v) => l.Name = v,
            ["contactInfo"] = (l, v) => l.ContactInfo = v,
            ["profile"] = (l, v) => l.Profile = v,
            ["data_quality"] = (l, v) => l.DataQuality = v,
            ["url"] = (l, v) => l.Urls.Add(v),
        };

        public LabelParser(string file) : base(file)
        {
        }

        protected override string MainNodeName => "label";
        protected override string[] TextNodes => new string[] { "id", "name", "contactInfo", "profile", "data_quality", "url" };
        protected override string[] SkipNodes => new string[] { "sublabels", "images" };

        protected override void SetNodeText(Label current, string nodeName, string text) {
#if (VERBOSE)
            if (nodeName == "id")
                System.Console.WriteLine($"[{nodeName, -20}] = {text}");
#endif
            if (current == null) return;
            Action<Label, string> setter;
            if (_setters.TryGetValue(nodeName, out setter)) {
                setter(current, text);
            }
        }
    }

}
