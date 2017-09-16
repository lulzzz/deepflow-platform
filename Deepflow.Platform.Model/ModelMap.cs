using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Model;
using Deepflow.Platform.Abstractions.Sources;

namespace Deepflow.Platform.Model
{
    public class ModelMap : IModelMap
    {
        public Dictionary<DataSource, Dictionary<SourceName, EntityAttribute>> SourceToModelMap { get; set; }
    }
}
