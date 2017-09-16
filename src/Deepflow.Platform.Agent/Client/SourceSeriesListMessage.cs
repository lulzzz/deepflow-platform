using System;
using Deepflow.Platform.Abstractions.Sources;

namespace Deepflow.Platform.Agent.Client
{
    public class SourceSeriesListMessage : IngestionClientMessage
    {
        public SourceSeriesList SourceSeriesList { get; set; }
    }
}