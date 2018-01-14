using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Deepflow.Common.Model
{
    public class ModelConfiguration
    {
        public List<EntityTypeModel> EntityTypes { get; set; } = new List<EntityTypeModel>();
        public List<EntityModel> Entities { get; set; } = new List<EntityModel>();
        public List<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();
        public List<CalculationModel> Calculations { get; set; } = new List<CalculationModel>();
        public List<EntityAttributeModel> EntityAttributes { get; set; } = new List<EntityAttributeModel>();
    }

    public class EntityTypeModel
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
    }

    public class EntityModel : IEquatable<EntityModel>
    {
        public Guid Guid { get; set; }
        public Guid EntityTypeGuid { get; set; }
        public string Name { get; set; }

        public bool Equals(EntityModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Guid.Equals(other.Guid) && EntityTypeGuid.Equals(other.EntityTypeGuid) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Guid.GetHashCode();
                hashCode = (hashCode * 397) ^ EntityTypeGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class AttributeModel : IEquatable<AttributeModel>
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public Guid EntityTypeGuid { get; set; }
        public string Unit { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as AttributeModel);
        }

        public bool Equals(AttributeModel other)
        {
            return other != null &&
                   Name == other.Name &&
                   Guid.Equals(other.Guid) &&
                   Unit == other.Unit;
        }

        public override int GetHashCode()
        {
            var hashCode = -1598768613;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(Guid);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Unit);
            return hashCode;
        }
    }

    public class CalculationModel : IEquatable<CalculationModel>
    {
        public Guid Guid { get; set; }
        public Guid EntityTypeGuid { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string RootExpression { get; set; }

        public bool Equals(CalculationModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Guid.Equals(other.Guid) && EntityTypeGuid.Equals(other.EntityTypeGuid) && string.Equals(Name, other.Name) && string.Equals(Unit, other.Unit) && string.Equals(RootExpression, other.RootExpression);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CalculationModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Guid.GetHashCode();
                hashCode = (hashCode * 397) ^ EntityTypeGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Unit != null ? Unit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RootExpression != null ? RootExpression.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class EntityAttributeModel : IEquatable<EntityAttributeModel>
    {
        public Guid EntityGuid { get; set; }
        public Guid AttributeGuid { get; set; }
        public string TagName { get; set; }

        public bool Equals(EntityAttributeModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EntityGuid.Equals(other.EntityGuid) && AttributeGuid.Equals(other.AttributeGuid) && string.Equals(TagName, other.TagName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityAttributeModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EntityGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ AttributeGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ (TagName != null ? TagName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttributeType
    {
        Attribute,
        Calculation
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityType
    {
        Well,
        Field,
        Manifold
    }
}
