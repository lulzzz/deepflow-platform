using System;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Abstractions.Sources
{
    public interface IDataSourcesController
    {
        Task<SourceSeriesList> GetSourceSeriesList(Guid dataSource);
        Task AddData(Guid dataSource, string sourceName, DataSourceDataPackage dataPackage);
    }

    public class DataSourceDataPackage
    {
        public AggregatedDataRange AggregatedRange { get; set; }
    }
}