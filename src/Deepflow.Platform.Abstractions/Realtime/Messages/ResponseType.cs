namespace Deepflow.Platform.Abstractions.Realtime.Messages
{
    public enum ResponseType
    {
        FetchAggregatedAttributeDataWithEdges,
        FetchRawSourceData,
        AddAggregatedAttributeData,
        AddAggregatedHistoricalAttributeData,
        UpdateAttributeSubscriptions
    }
}