using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Series.Attribute
{
    public interface IAttributeDataProvider
    {
        Task<List<AggregatedDataRange>> GetData(TimeRange timeRange, int aggregationSeconds);
        Task<List<TimeRange>> GetTimeRanges(int aggregationSeconds);
        Task<List<AggregatedDataRange>> AddData(AggregatedDataRange dataRange);
    }
}