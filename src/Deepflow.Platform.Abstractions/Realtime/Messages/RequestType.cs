namespace Deepflow.Platform.Abstractions.Realtime.Messages
{
    public enum RequestType
    {
        FetchAggregatedAttributeDataWithEdges,
        FetchRawSourceData,
        UpdateAttributeSubscriptions,
        AddAggregatedAttributeData,
        AddAggregatedHistoricalAttributeData
    }
}