using System;
using System.Collections.Generic;

namespace XmlPerformance
{
    [System.Diagnostics.DebuggerDisplay("{Id} - {Name} ({DataQuality})")]
    class Artist : IAcceptVisitor<IVisitor>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RealName { get; set; }
        public string Profile { get; set; }
        public string DataQuality { get; set; } // TODO: enum?
        public ICollection<string> Urls { get; } = new List<string>();
        public ICollection<string> Aliases { get; } = new List<string>();
        public ICollection<string> NameVariations { get; } = new List<string>();
        public ICollection<string> Members { get; } = new List<string>();
        public ICollection<string> Groups { get; } = new List<string>();

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    class ArtistParser : Parser<Artist>
    {
        public ArtistParser(string file) : base(file)
        {
        }

        protected override string MainNodeName => "artist";

        protected override string[] TextNodes { get; } = new string[] { "id", "name", "realname", "profile", "data_quality", "url" };

        protected override string[] SkipNodes { get; } = new string[] { "images", "namevariations", "aliases", "groups", "members" };

        protected override IDictionary<string, Action<Artist, string>> ElementSetters { get; } = new Dictionary<string, Action<Artist, string>>(StringComparer.OrdinalIgnoreCase) {
            ["id"] = (l, v) => l.Id = int.Parse(v),
            ["name"] = (l, v) => l.Name = v,
            ["realname"] = (l, v) => l.RealName = v,
            ["profile"] = (l, v) => l.Profile = v,
            ["data_quality"] = (l, v) => l.DataQuality = v,
            ["url"] = (l, v) => l.Urls.Add(v),
        };
    }
}
