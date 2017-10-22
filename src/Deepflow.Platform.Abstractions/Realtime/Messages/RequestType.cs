namespace Deepflow.Platform.Abstractions.Realtime.Messages
{
    public enum RequestType
    {
        FetchAggregatedAttributeData,
        FetchRawSourceData,
        UpdateAttributeSubscriptions,
        AddAggregatedAttributeData,
        AddAggregatedHistoricalAttributeData
    }
}