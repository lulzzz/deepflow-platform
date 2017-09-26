using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Deepflow.Platform.Core.Tools
{
    public static class JsonSettings
    {
        public static readonly JsonSerializerSettings Setttings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } } };
    }
}
