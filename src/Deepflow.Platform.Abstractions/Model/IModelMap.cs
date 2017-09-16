using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Sources;

namespace Deepflow.Platform.Abstractions.Model
{
    public interface IModelMap
    {
        Dictionary<DataSource, Dictionary<SourceName, EntityAttribute>> SourceToModelMap { get; set; }
    }
}