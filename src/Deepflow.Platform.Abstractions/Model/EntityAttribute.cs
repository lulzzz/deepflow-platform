using System;

namespace Deepflow.Platform.Abstractions.Model
{
    public class EntityAttribute
    {
        public Guid Entity { get; set; }
        public Guid Attribute { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as EntityAttribute;
            return other?.Entity == Entity && other?.Attribute == Attribute;
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode() ^ Attribute.GetHashCode();
        }
    }
}
