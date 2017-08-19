using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Controllers
{
    public class DataResponse
    {
        public int Id { get; set; }
        public DataRange Data { get; set; }
    }
}
