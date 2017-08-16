namespace Deepflow.Platform.Abstractions.Series
{
    public class AggregatedDataRange
    {
        public int AggregationSeconds { get; set; }
        public TimeRange TimeRange { get; set; }
        public double[] Data { get; set; }
    }
}
