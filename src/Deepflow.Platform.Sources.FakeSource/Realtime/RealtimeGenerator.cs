using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Sources.FakeSource.Data;

namespace Deepflow.Platform.Sources.FakeSource.Realtime
{
    public class RealtimeGenerator
    {
        private readonly string _sourceName;
        private readonly int _intervalSeconds;
        private readonly Action<DataRange> _onPoint;
        private static readonly SHA1 Sha1 = SHA1.Create();
        private readonly DataGenerator _generator;
        private readonly int _offsetSeconds;
        private CancellationTokenSource _cancellationTokenSource;

        public RealtimeGenerator(string sourceName, int intervalSeconds, Action<DataRange> onPoint)
        {
            _sourceName = sourceName;
            _intervalSeconds = intervalSeconds;
            _onPoint = onPoint;
            _generator = new DataGenerator();

            var hash = Sha1.ComputeHash(Encoding.UTF8.GetBytes(sourceName));
            var seed = BitConverter.ToInt32(hash, 0);
            _offsetSeconds = new Random(seed).Next(0, intervalSeconds);
        }

        public void Start()
        {
            if (_cancellationTokenSource != null)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => PointLoop(), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        private async void PointLoop()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var seconds = DateTime.Now.SecondsSince1970Utc(); // + _offsetSeconds;
                    var nextQuantisedSeconds = seconds - (seconds % _intervalSeconds) + _intervalSeconds;
                    var millisecondsUntilNextPoint = (int)(nextQuantisedSeconds - seconds) * 1000;
                    await Task.Delay(millisecondsUntilNextPoint, _cancellationTokenSource.Token);

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    var point = _generator.GenerateData(_sourceName, new TimeRange(nextQuantisedSeconds, nextQuantisedSeconds), _intervalSeconds);
                    _onPoint(point);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    }
}
