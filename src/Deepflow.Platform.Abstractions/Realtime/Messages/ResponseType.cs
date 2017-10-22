namespace Deepflow.Platform.Abstractions.Realtime.Messages
{
    public enum ResponseType
    {
        FetchAggregatedAttributeData,
        FetchRawSourceData,
        AddAggregatedAttributeData,
        AddAggregatedHistoricalAttributeData,
        UpdateAttributeSubscriptions
    }
}