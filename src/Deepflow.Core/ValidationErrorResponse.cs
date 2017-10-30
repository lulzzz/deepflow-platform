using System.Collections.Generic;
using Newtonsoft.Json;

namespace Deepflow.Core
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ValidationErrorResponse : ErrorResponse
    {
        [JsonProperty(PropertyName = "errors")]
        public IEnumerable<Item> Errors { get; set; }

        public ValidationErrorResponse(string message, string description) : base(message, description)
        { }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class Item
        {
            [JsonProperty(PropertyName = "fieldName")]
            public string FieldName { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }
        }
    }
}