using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Model
{
    public interface IModelMapProvider
    {
        Task<SeriesModelMapping> GetSeriesModelMapping(Guid dataSource, string sourceName);
        Task<IEnumerable<string>> GetSourceNamesForDataSource(Guid dataSource);
    }
}