using System;

namespace Deepflow.Platform.Abstractions.Model
{
    public class SeriesModelMapping
    {
        public Guid Entity { get; set; }
        public Guid Attribute { get; set; }
        public Guid DataSource { get; set; }
        public string SourceName { get; set; }

        public SeriesModelMapping(Guid entity, Guid attribute, Guid dataSource, string sourceName)
        {
            Entity = entity;
            Attribute = attribute;
            DataSource = dataSource;
            SourceName = sourceName;
        }
    }
}
