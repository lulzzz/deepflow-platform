using System;
using System.Collections.Generic;
using System.Text;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    /// <summary>
    /// Tests the time weighted average aggregation
    /// </summary>
    public class DataAggregatorTests
    {
        private static readonly Mock<ILogger<DataAggregator>> LoggerMock = new Mock<ILogger<DataAggregator>>();
        private readonly DataAggregator _aggregator = new DataAggregator(LoggerMock.Object);

        [Fact]
        public void TestNotFull()
        {
            var input = new AggregatedDataRange(300, 400, new List<double> { 350, 35, 400, 40 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(200, 400), 200);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(200, 400, new List<double> { 400, 37.5 }, 200) };
            actual.IsSameOrEqualTo(expected);
        }

        /*[Fact]
        public void TestNoPoints()
        {
            var input = new AggregatedDataRange(300, 600, new List<double>(), 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestOnePointAtTheStart()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 350, 35 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 35 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestOnePointInTheMiddle()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 450, 35 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 35 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestOnePointAtTheEnd()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 600, 35 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 35 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestOnePointAtTheStartAndEnd()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 350, 35, 600, 65 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 40 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestOnePointAtTheStartMiddleAndEnd()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 350, 35, 450, 45, 600, 65 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 45 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestOnePointAtTheMiddleAndEnd()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 450, 45, 600, 65 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 50 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestPointsEverywhere()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 350, 35, 400, 40, 450, 45, 500, 50, 550, 55, 600, 63 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 48 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestPointsEverywhereButStart()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 400, 40, 450, 45, 500, 50, 550, 55, 600, 60 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 50 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestPointsEverywhereButEnd()
        {
            var input = new AggregatedDataRange(300, 600, new List<double> { 350, 25, 400, 40, 450, 45, 500, 50, 550, 55 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 600), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 600, new List<double> { 600, 45 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestTwoChunksPointsStartAndEnd()
        {
            var input = new AggregatedDataRange(300, 900, new List<double> { 350, 35, 900, 95 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 900), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 900, new List<double> { 600, 35, 900, 45 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestTwoChunksPointsStartFirstEndAndSecondEnd()
        {
            var input = new AggregatedDataRange(300, 900, new List<double> { 350, 35, 600, 65, 900, 95 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 900), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 900, new List<double> { 600, 40, 900, 70 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestTwoChunksPointsStartSecondStartAndSecondEnd()
        {
            var input = new AggregatedDataRange(300, 900, new List<double> { 350, 35, 650, 65, 900, 95 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 900), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 900, new List<double> { 600, 35, 900, 70 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestTwoChunksPointsAll()
        {
            var input = new AggregatedDataRange(300, 900, new List<double> { 350, 35, 400, 40, 450, 45, 500, 50, 550, 55, 600, 63, 650, 65, 700, 70, 750, 75, 800, 80, 850, 85, 900, 93 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 900), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 900, new List<double> { 600, 48, 900, 78 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestThreeChunksStartEmpty()
        {
            var input = new AggregatedDataRange(600, 1200, new List<double> { 650, 65, 700, 70, 750, 75, 800, 80, 850, 85, 900, 93, 950, 95, 1000, 100, 1050, 105, 1100, 110, 1150, 115, 1200, 123 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 1200), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(600, 1200, new List<double> { 900, 78, 1200, 108 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestThreeChunksMiddleEmpty()
        {
            var input = new AggregatedDataRange(300, 1200, new List<double> { 350, 35, 400, 40, 450, 45, 500, 50, 550, 55, 600, 63, 950, 95, 1000, 100, 1050, 105, 1100, 110, 1150, 115, 1200, 123 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 1200), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 1200, new List<double> { 600, 48, 1200, 108 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }

        [Fact]
        public void TestThreeChunksEndEmpty()
        {
            var input = new AggregatedDataRange(300, 1200, new List<double> { 350, 35, 400, 40, 450, 45, 500, 50, 550, 55, 600, 63, 650, 65, 700, 70, 750, 75, 800, 80, 850, 85, 900, 93 }, 50);
            var actual = _aggregator.Aggregate(new List<AggregatedDataRange> { input }, new TimeRange(300, 1200), 50, 300);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(300, 1200, new List<double> { 600, 48, 900, 78 }, 300) };
            actual.IsSameOrEqualTo(expected);
        }*/
    }
}
