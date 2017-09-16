using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Deepflow.Platform.Core.Tools
{
    public class RetryingHttpClient : HttpClient
    {
        private readonly int _delaySeconds;
        private readonly ILogger _logger;

        // Handle both exceptions and return values in one policy
        private static readonly HashSet<HttpStatusCode> HttpStatusCodesWorthRetrying = new HashSet<HttpStatusCode>
        {
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        };

        private readonly RetryPolicy<HttpResponseMessage> _retryPolicy;

        public RetryingHttpClient(int retryCount, int delaySeconds, string task, ILogger logger)
        {
            _delaySeconds = delaySeconds;
            _logger = logger;
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => HttpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryAsync(retryCount, (i, context) => TimeSpan.FromSeconds(delaySeconds), OnRetry(task));
        }

        private Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context> OnRetry(string task)
        {
            return (result, time, retryCount, context) =>
            {
                if (result.Exception != null)
                {
                    _logger.LogDebug(null, result.Exception, task + $" due to error, trying retry {retryCount} in {_delaySeconds} seconds...");
                }
                if (result.Result != null && HttpStatusCodesWorthRetrying.Contains(result.Result.StatusCode))
                {
                    _logger.LogDebug(null, result.Exception, task + $" due to bad HTTP status code {result.Result.StatusCode}, trying retry {retryCount} in {_delaySeconds} seconds...");
                }
            };
        }

        public RetryingHttpClient(int delaySeconds)
        {
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => HttpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryForever((i, context) => TimeSpan.FromSeconds(delaySeconds));
        }

        public new Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        {
            return _retryPolicy.ExecuteAsync(() => base.PostAsync(requestUri, content));
        }
    }
}
