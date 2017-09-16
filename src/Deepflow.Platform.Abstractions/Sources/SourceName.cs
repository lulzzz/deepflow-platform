namespace Deepflow.Platform.Abstractions.Sources
{
    public class SourceName
    {
        public string Name { get; set; }

        public SourceName(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            return (obj as SourceName)?.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
