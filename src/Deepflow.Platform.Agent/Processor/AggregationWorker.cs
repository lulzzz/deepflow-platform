using System;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Agent.Provider;
using Deepflow.Platform.Core.Tools;
using Polly;

namespace Deepflow.Platform.Agent.Processor
{
    public class AggregationWorker : IDisposable
    {
        private readonly string _sourceName;
        private readonly int _aggregationSeconds;
        private readonly int _sourceDelaySeconds;
        private readonly ISourceDataProvider _provider;
        private readonly Func<AggregatedDataRange, Task> _onPoint;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private int _lastRealtimeTimeSeconds = 0;
        private int _lastAggregatedTimeSeconds = 0;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly Polly.Retry.RetryPolicy _retryPolicy;

        public AggregationWorker(string sourceName, int aggregationSeconds, int sourceDelaySeconds, ISourceDataProvider provider, Func<AggregatedDataRange, Task> onPoint)
        {
            _sourceName = sourceName;
            _aggregationSeconds = aggregationSeconds;
            _sourceDelaySeconds = sourceDelaySeconds;
            _provider = provider;
            _onPoint = onPoint;

            // Retry a specified number of times, using a function to 
            // calculate the duration to wait between retries based on 
            // the current retry attempt (allows for exponential backoff)
            // In this case will wait for
            //  2 ^ 1 = 2 seconds then
            //  2 ^ 2 = 4 seconds then
            //  2 ^ 3 = 8 seconds then
            //  2 ^ 4 = 16 seconds then
            //  2 ^ 5 = 32 seconds
            _retryPolicy = Policy
                .Handle<Exception>(exception => exception.GetType() != typeof(TaskCanceledException))
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, Math.Min(retryAttempt, 5))));

            Task.Run(TimerLoop);
        }

        private async Task TimerLoop()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    if (_tokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    var seconds = (int) DateTime.Now.SecondsSince1970Utc(); // + _offsetSeconds;
                    var nextQuantisedSeconds = GetNextQuantisedSeconds(seconds);
                    var delayedSeconds = nextQuantisedSeconds + _sourceDelaySeconds;
                    var millisecondsUntilNextPoint = (delayedSeconds - seconds) * 1000;
                    if (millisecondsUntilNextPoint > 0)
                    {
                        await Task.Delay(millisecondsUntilNextPoint, _tokenSource.Token);
                    }

                    if (_tokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    await FetchIfNecessary(nextQuantisedSeconds);
                });

                if (_tokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        public async Task NotifyRealtimeRaw(int seconds)
        {
            if (_lastRealtimeTimeSeconds != 0 && !AreTimesInSameSegement(_lastRealtimeTimeSeconds, seconds))
            {
                var nextQuantisedSeconds = GetNextQuantisedSeconds(_lastRealtimeTimeSeconds);
                await FetchIfNecessary(nextQuantisedSeconds);
            }
            _lastRealtimeTimeSeconds = seconds;
        }

        private async Task FetchIfNecessary(int seconds)
        {
            if (_lastAggregatedTimeSeconds < seconds)
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (_lastAggregatedTimeSeconds < seconds)
                    {
                        await FetchAggregationPoint(seconds);
                        _lastAggregatedTimeSeconds = seconds;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private int GetNextQuantisedSeconds(int seconds)
        {
            return GetQuantisedSeconds(seconds) + _aggregationSeconds;
        }

        private int GetQuantisedSeconds(int seconds)
        {
            return seconds - (seconds % _aggregationSeconds);
        }

        private bool AreTimesInSameSegement(int secondsOne, int secondsTwo)
        {
            var oneQuantised = GetQuantisedSeconds(secondsOne);
            var twoQuantised = GetQuantisedSeconds(secondsTwo);
            return oneQuantised == twoQuantised;
        }

        private async Task FetchAggregationPoint(int milliseconds)
        {
            var aggregatedTimeRange = new TimeRange(milliseconds - _aggregationSeconds, milliseconds);
            var aggregatedData = await _provider.FetchAggregatedData(_sourceName, aggregatedTimeRange, _aggregationSeconds);
            await _onPoint(aggregatedData);
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
        }
    }
}
