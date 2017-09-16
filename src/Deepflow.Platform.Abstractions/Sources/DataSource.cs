using System;

namespace Deepflow.Platform.Abstractions.Sources
{
    public class DataSource
    {
        public Guid Guid { get; set; }

        public DataSource(Guid guid)
        {
            Guid = guid;
        }

        public override bool Equals(object obj)
        {
            return (obj as DataSource)?.Guid == Guid;
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}
