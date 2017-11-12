using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Processing;
using Deepflow.Ingestion.Service.Realtime;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Common.Data.Caching;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Concurrency;
using Xunit;
using SeriesConfiguration = Deepflow.Common.Model.SeriesConfiguration;

namespace Deepflow.Ingestion.Service.Tests
{
    public class IngestionProcessorTests
    {
        /*private readonly IngestionProcessor _processor;
        private readonly Mock<IPersistentDataProvider> _persistenceMock;
        private readonly Mock<IModelProvider> _modelMock;
        private readonly Mock<IDataMessenger> _messengerMock;

        private readonly Guid _entity = Guid.NewGuid();
        private readonly Guid _attribute = Guid.NewGuid();
        private readonly Guid _series50 = Guid.NewGuid();
        private readonly Guid _series100 = Guid.NewGuid();
        private readonly Guid _series200 = Guid.NewGuid();

        public IngestionProcessorTests()
        {
            _persistenceMock = new Mock<IPersistentDataProvider>();
            _modelMock = new Mock<IModelProvider>();
            _messengerMock = new Mock<IDataMessenger>();
            _modelMock.Setup(x => x.ResolveSeries(_entity, _attribute, 50)).Returns(Task.FromResult(_series50));
            _modelMock.Setup(x => x.ResolveSeries(_entity, _attribute, 100)).Returns(Task.FromResult(_series100));
            _modelMock.Setup(x => x.ResolveSeries(_entity, _attribute, 200)).Returns(Task.FromResult(_series200));

            var filterer = new RangeFilterer<AggregatedDataRange>(new AggregatedRangeCreator(), new AggregateRangeFilteringPolicy(), new AggregatedRangeAccessor());
            var merger = new RangeMerger<AggregatedDataRange>(filterer, new RangeJoiner<AggregatedDataRange>(new AggregatedRangeCreator(), new AggregatedRangeAccessor(), new Mock<ILogger<RangeJoiner<AggregatedDataRange>>>().Object),  new AggregatedRangeAccessor());
            var timeFilterer = new RangeFilterer<TimeRange>(new TimeRangeCreator(), new TimeRangeFilteringPolicy(), new TimeRangeAccessor());
            var timeMerger = new RangeMerger<TimeRange>(timeFilterer, new RangeJoiner<TimeRange>(new TimeRangeCreator(), new TimeRangeAccessor(), new Mock<ILogger<RangeJoiner<TimeRange>>>().Object), new TimeRangeAccessor());
            _processor = new IngestionProcessor(_persistenceMock.Object, new DataAggregator(new Mock<ILogger<DataAggregator>>().Object), _modelMock.Object, _messengerMock.Object, merger, timeMerger, filterer, new SeriesConfiguration { AggregationsSeconds = new [] { 50, 100, 200 }}, new Mock<ILogger<IngestionProcessor>>().Object, new TripCounterFactory(new Mock<ILogger<TripCounter>>().Object));
        }
        
        [Fact]
        public async Task TestIngestHistoricalFresh()
        {
            _persistenceMock.Setup(x => x.GetData(_series50, new TimeRange(200, 400))).Returns(Task.FromResult((IEnumerable<AggregatedDataRange>)new List<AggregatedDataRange>()));
            _persistenceMock.Setup(x => x.GetAllTimeRanges(_series50)).Returns(Task.FromResult((IEnumerable<TimeRange>)new List<TimeRange>()));
            IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)> saved = null;
            _persistenceMock.Setup(x => x.SaveData(It.IsAny<IEnumerable<(Guid entity, Guid attribute, int aggregationSeconds, IEnumerable<AggregatedDataRange> dataRanges)>>())).Returns(Task.CompletedTask).Callback((IEnumerable<(Guid entity, Guid attribute, int aggregationSeconds, IEnumerable<AggregatedDataRange> dataRanges)> savedData) => saved = savedData);

            var aggregatedDataRange = new AggregatedDataRange(300, 400, new List<double> { 350, 35, 400, 40 }, 50);
            await _processor.ReceiveHistoricalData(_entity, _attribute, aggregatedDataRange);

            _persistenceMock.Verify(x => x.SaveTimeRanges(_series50, new List<TimeRange>{ new TimeRange(300, 400) }));
            saved.ElementAt(0).series.ShouldBeEquivalentTo(_series50);
            saved.ElementAt(0).dataRanges.ShouldBeEquivalentTo(new List<AggregatedDataRange> { new AggregatedDataRange(300, 400, new List<double> { 350, 35, 400, 40 }, 50) });
            saved.ElementAt(1).series.ShouldBeEquivalentTo(_series100);
            saved.ElementAt(1).dataRanges.ShouldBeEquivalentTo(new List<AggregatedDataRange> { new AggregatedDataRange(300, 400, new List<double> { 400, 37.5 }, 100) });
            saved.ElementAt(2).series.ShouldBeEquivalentTo(_series200);
            saved.ElementAt(2).dataRanges.ShouldBeEquivalentTo(new List<AggregatedDataRange> { new AggregatedDataRange(200, 400, new List<double> { 400, 37.5 }, 200) });
        }

        [Fact]
        public async Task TestIngestHistoricalBefore()
        {
            var existingTimeRanges = new List<TimeRange> { new TimeRange(500, 600) };
            var existingDataRanges = new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50) };
            var newDataRange = new AggregatedDataRange(300, 400, new List<double> { 350, 35, 400, 40 }, 50);
            var expectedDataRanges = new Dictionary<Guid, List<AggregatedDataRange>>
            {
                { _series50, new List<AggregatedDataRange> { new AggregatedDataRange(300, 400, new List<double> { 350, 35, 400, 40 }, 50) } },
                { _series100, new List<AggregatedDataRange> { new AggregatedDataRange(300, 400, new List<double> { 400, 37.5 }, 100) } },
                { _series200, new List<AggregatedDataRange> { new AggregatedDataRange(200, 400, new List<double> { 400, 37.5 }, 200) } }
            };
            var expectedTimeRanges = new List<TimeRange> { new TimeRange(300, 400), new TimeRange(500, 600) };

            await RunReceiveHistoricalTest(existingDataRanges, existingTimeRanges, new TimeRange(200, 400), newDataRange, expectedTimeRanges, expectedDataRanges);
        }

        [Fact]
        public async Task TestIngestHistoricalTouchingStart()
        {
            var existingTimeRanges = new List<TimeRange> { new TimeRange(500, 600) };
            var existingDataRanges = new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50) };
            var newDataRange = new AggregatedDataRange(400, 500, new List<double> { 450, 45, 500, 50 }, 50);
            var quantisedTimeRange = new TimeRange(400, 600);
            var expectedDataRanges = new Dictionary<Guid, List<AggregatedDataRange>>
            {
                { _series50, new List<AggregatedDataRange> { new AggregatedDataRange(400, 500, new List<double> { 450, 45, 500, 50 }, 50) } },
                { _series100, new List<AggregatedDataRange> { new AggregatedDataRange(400, 500, new List<double> { 500, 47.5 }, 100) } },
                { _series200, new List<AggregatedDataRange> { new AggregatedDataRange(400, 600, new List<double> { 600, 52.5 }, 200) } }
            };
            var expectedTimeRanges = new List<TimeRange> { new TimeRange(400, 600) };

            await RunReceiveHistoricalTest(existingDataRanges, existingTimeRanges, quantisedTimeRange, newDataRange, expectedTimeRanges, expectedDataRanges);
        }

        [Fact]
        public async Task TestIngestHistoricalOverlappingStart()
        {
            var existingTimeRanges = new List<TimeRange> { new TimeRange(500, 600) };
            var existingDataRanges = new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50) };
            var newDataRange = new AggregatedDataRange(400, 550, new List<double> { 450, 45, 500, 50, 550, 56 }, 50);
            var quantisedTimeRange = new TimeRange(400, 600);
            var expectedDataRanges = new Dictionary<Guid, List<AggregatedDataRange>>
            {
                { _series50, new List<AggregatedDataRange> { new AggregatedDataRange(400, 550, new List<double> { 450, 45, 500, 50, 550, 56 }, 50) } },
                { _series100, new List<AggregatedDataRange> { new AggregatedDataRange(400, 600, new List<double> { 500, 47.5, 600, 58 }, 100) } },
                { _series200, new List<AggregatedDataRange> { new AggregatedDataRange(400, 600, new List<double> { 600, 52.75 }, 200) } }
            };
            var expectedTimeRanges = new List<TimeRange> { new TimeRange(400, 600) };

            await RunReceiveHistoricalTest(existingDataRanges, existingTimeRanges, quantisedTimeRange, newDataRange, expectedTimeRanges, expectedDataRanges);
        }

        [Fact]
        public async Task TestIngestHistoricalEqualTo()
        {
            var existingTimeRanges = new List<TimeRange> { new TimeRange(500, 600) };
            var existingDataRanges = new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50) };
            var newDataRange = new AggregatedDataRange(500, 600, new List<double> { 550, 56, 600, 61 }, 50);
            var quantisedTimeRange = new TimeRange(400, 600);
            var expectedDataRanges = new Dictionary<Guid, List<AggregatedDataRange>>
            {
                { _series50, new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 550, 56, 600, 61 }, 50) } },
                { _series100, new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 600, 58.5 }, 100) } },
                { _series200, new List<AggregatedDataRange> { new AggregatedDataRange(400, 600, new List<double> { 600, 58.5 }, 200) } }
            };
            var expectedTimeRanges = new List<TimeRange> { new TimeRange(500, 600) };

            await RunReceiveHistoricalTest(existingDataRanges, existingTimeRanges, quantisedTimeRange, newDataRange, expectedTimeRanges, expectedDataRanges);
        }

        [Fact]
        public async Task TestIngestHistoricalOverlappingEnd()
        {
            var existingTimeRanges = new List<TimeRange> { new TimeRange(500, 600) };
            var existingDataRanges = new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50) };
            var newDataRange = new AggregatedDataRange(550, 650, new List<double> { 600, 61, 650, 66 }, 50);
            var quantisedTimeRange = new TimeRange(400, 800);
            var expectedDataRanges = new Dictionary<Guid, List<AggregatedDataRange>>
            {
                { _series50, new List<AggregatedDataRange> { new AggregatedDataRange(550, 650, new List<double> { 600, 61, 650, 66 }, 50) } },
                { _series100, new List<AggregatedDataRange> { new AggregatedDataRange(500, 700, new List<double> { 600, 58, 700, 66 }, 100) } },
                { _series200, new List<AggregatedDataRange> { new AggregatedDataRange(400, 800, new List<double> { 600, 58, 800, 66 }, 200) } }
            };
            var expectedTimeRanges = new List<TimeRange> { new TimeRange(500, 650) };

            await RunReceiveHistoricalTest(existingDataRanges, existingTimeRanges, quantisedTimeRange, newDataRange, expectedTimeRanges, expectedDataRanges);
        }

        [Fact]
        public async Task TestIngestHistoricalTouchingEnd()
        {
            var existingTimeRanges = new List<TimeRange> { new TimeRange(500, 600) };
            var existingDataRanges = new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50) };
            var newDataRange = new AggregatedDataRange(600, 700, new List<double> { 650, 66, 700, 71 }, 50);
            var quantisedTimeRange = new TimeRange(400, 800);
            var expectedDataRanges = new Dictionary<Guid, List<AggregatedDataRange>>
            {
                { _series50, new List<AggregatedDataRange> { new AggregatedDataRange(600, 700, new List<double> { 650, 66, 700, 71 }, 50) } },
                { _series100, new List<AggregatedDataRange> { new AggregatedDataRange(600, 700, new List<double> { 700, 68.5 }, 100) } },
                { _series200, new List<AggregatedDataRange> { new AggregatedDataRange(600, 800, new List<double> { 800, 68.5 }, 200) } }
            };
            var expectedTimeRanges = new List<TimeRange> { new TimeRange(500, 700) };

            await RunReceiveHistoricalTest(existingDataRanges, existingTimeRanges, quantisedTimeRange, newDataRange, expectedTimeRanges, expectedDataRanges);
        }

        [Fact]
        public async Task TestIngestHistoricalAfterEnd()
        {
            var existingTimeRanges = new List<TimeRange> { new TimeRange(500, 600) };
            var existingDataRanges = new List<AggregatedDataRange> { new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50) };
            var newDataRange = new AggregatedDataRange(700, 800, new List<double> { 750, 76, 800, 81 }, 50);
            var quantisedTimeRange = new TimeRange(400, 800);
            var expectedDataRanges = new Dictionary<Guid, List<AggregatedDataRange>>
            {
                { _series50, new List<AggregatedDataRange> { new AggregatedDataRange(700, 800, new List<double> { 750, 76, 800, 81 }, 50) } },
                { _series100, new List<AggregatedDataRange> { new AggregatedDataRange(700, 800, new List<double> { 800, 78.5 }, 100) } },
                { _series200, new List<AggregatedDataRange> { new AggregatedDataRange(600, 800, new List<double> { 800, 78.5 }, 200) } }
            };
            var expectedTimeRanges = new List<TimeRange> { new TimeRange(500, 600), new TimeRange(700, 800) };

            await RunReceiveHistoricalTest(existingDataRanges, existingTimeRanges, quantisedTimeRange, newDataRange, expectedTimeRanges, expectedDataRanges);
        }

        private async Task RunReceiveHistoricalTest(List<AggregatedDataRange> existingDataRanges, List<TimeRange> existingTimeRanges, TimeRange quantisedTimeRange, AggregatedDataRange newDataRange, List<TimeRange> expectedTimeRanges, Dictionary<Guid, List<AggregatedDataRange>> expectedDataRanges)
        {
            _persistenceMock.Setup(x => x.GetData(_series50, quantisedTimeRange))
                .Returns(Task.FromResult((IEnumerable<AggregatedDataRange>) existingDataRanges));
            _persistenceMock.Setup(x => x.GetAllTimeRanges(_series50))
                .Returns(Task.FromResult((IEnumerable<TimeRange>) existingTimeRanges));
            IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)> saved = null;
            _persistenceMock
                .Setup(x => x.SaveData(It.IsAny<IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)>>()))
                .Returns(Task.CompletedTask)
                .Callback(
                    (IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)> savedData) => saved = savedData);

            await _processor.ReceiveHistoricalData(_entity, _attribute, newDataRange);

            _persistenceMock.Verify(x => x.SaveTimeRanges(_series50, expectedTimeRanges));
            var index = 0;
            foreach (var expectedDataRange in expectedDataRanges)
            {
                var element = saved.ElementAt(index++);
                element.series.ShouldBeEquivalentTo(expectedDataRange.Key);
                element.dataRanges.ShouldBeEquivalentTo(expectedDataRange.Value);
            }
        }*/

        /* [Fact]
         public async Task TestIngestHistoricalBefore()
         {
             var existing50 = new AggregatedDataRange(300, 400, new List<double>(), );

             _persistenceMock.Setup(x => x.GetData(_series50, new TimeRange(200, 400))).Returns(Task.FromResult((IEnumerable<AggregatedDataRange>)new List<AggregatedDataRange>()));
             _persistenceMock.Setup(x => x.GetAllTimeRanges(_series50)).Returns(Task.FromResult((IEnumerable<TimeRange>)new List<TimeRange>()));
             _persistenceMock.Setup(x => x.SaveData(It.IsAny<IEnumerable<(Guid series, IEnumerable<AggregatedDataRange> dataRanges)>>())).Returns(Task.CompletedTask);

             var aggregatedDataRange = new AggregatedDataRange(300, 400, new List<double> { 350, 35, 400, 40 }, 50);
             await _processor.ReceiveHistoricalData(_entity, _attribute, aggregatedDataRange);

             _persistenceMock.Verify(x => x.SaveTimeRanges(_series50, new List<TimeRange> { new TimeRange(300, 400) }));
         }*/
    }
}

