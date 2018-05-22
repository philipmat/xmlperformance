namespace XmlPerformance
{
    class Artist : IAcceptVisitor<IVisitor>
    {
        public string DataQuality { get; internal set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
