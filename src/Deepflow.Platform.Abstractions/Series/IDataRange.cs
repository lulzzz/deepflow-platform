using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataRange
    {
        TimeRange TimeRange { get; }
        List<double> Data { get; }
    }

    public interface IDataRange<TRange, TCreator> : IDataRange where TCreator : IRangeCreator<TRange> where TRange: IDataRange
    {
    }
}