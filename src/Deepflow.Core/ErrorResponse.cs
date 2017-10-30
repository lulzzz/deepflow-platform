using Newtonsoft.Json;

namespace Deepflow.Core
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ErrorResponse
    {
        public ErrorResponse(string name, string description)
        {
            Name = name;
            Description = description;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; private set; }
    }
}