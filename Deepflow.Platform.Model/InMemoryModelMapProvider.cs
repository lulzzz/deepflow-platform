using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Model;
using Deepflow.Platform.Abstractions.Sources;

namespace Deepflow.Platform.Model
{
    public class InMemoryModelMapProvider : IModelMapProvider
    {
        private readonly IModelMap _modelMap;

        public InMemoryModelMapProvider(IModelMap modelMap)
        {
            _modelMap = modelMap;
        }

        public Task<SeriesModelMapping> GetSeriesModelMapping(Guid dataSource, string sourceName)
        {
            if (!_modelMap.SourceToModelMap.TryGetValue(new DataSource(dataSource), out Dictionary<SourceName, EntityAttribute> sourceMap))
            {
                return null;
            }

            if (!sourceMap.TryGetValue(new SourceName(sourceName), out EntityAttribute entityAttribute))
            {
                return null;
            }

            return Task.FromResult(new SeriesModelMapping(entityAttribute.Entity, entityAttribute.Attribute, dataSource, sourceName));
        }
    }
}
