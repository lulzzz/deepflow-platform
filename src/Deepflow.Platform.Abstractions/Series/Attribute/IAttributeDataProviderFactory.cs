using System;

namespace Deepflow.Platform.Abstractions.Series.Attribute
{
    public interface IAttributeDataProviderFactory
    {
        IAttributeDataProvider Create(Guid entity, Guid attribute);
    }
}