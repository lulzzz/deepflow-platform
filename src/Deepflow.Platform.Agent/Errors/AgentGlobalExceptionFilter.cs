using System;
using System.Collections.Generic;
using Deepflow.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Agent.Errors
{
    public class AgentGlobalExceptionFilter : GlobalExceptionFilter
    {
        public AgentGlobalExceptionFilter(ILogger<AgentGlobalExceptionFilter> logger) : base(logger)
        {
        }

        protected override IDictionary<Type, Func<Exception, ObjectResult>> CustomExceptionHandlers { get; set; } = new Dictionary<Type, Func<Exception, ObjectResult>>();
    }
}