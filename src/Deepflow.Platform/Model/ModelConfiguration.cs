using System.Collections.Generic;

namespace Deepflow.Platform.Model
{
    public class ModelConfiguration
    {
        public IEnumerable<string> Entities { get; set; }
        public IEnumerable<string> Attributes { get; set; }
    }
}
