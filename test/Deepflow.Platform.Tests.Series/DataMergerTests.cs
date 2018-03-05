using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    public class DataMergerTests
    {
        private static readonly RangeFilterer<TimeRange> _filterer = new RangeFilterer<TimeRange>(new TimeRangeCreator(), new TimeRangeFilteringPolicy(), new TimeRangeAccessor());
        private static readonly RangeJoiner<TimeRange> _joiner = new RangeJoiner<TimeRange>(new TimeRangeCreator(), new TimeRangeAccessor(), new Mock<ILogger<RangeJoiner<TimeRange>>>().Object);
        private readonly RangeMerger<TimeRange> _merger = new RangeMerger<TimeRange>(_filterer, _joiner, new TimeRangeAccessor());

        private static readonly RangeFilterer<AggregatedDataRange> _aggregatedFilterer = new RangeFilterer<AggregatedDataRange>(new AggregatedRangeCreator(), new AggregateRangeFilteringPolicy(), new AggregatedRangeAccessor());
        private static readonly RangeJoiner<AggregatedDataRange> _aggregatedJoiner = new RangeJoiner<AggregatedDataRange>(new AggregatedRangeCreator(), new AggregatedRangeAccessor(), new Mock<ILogger<RangeJoiner<AggregatedDataRange>>>().Object);
        private readonly RangeMerger<AggregatedDataRange> _aggregatedMerger = new RangeMerger<AggregatedDataRange>(_aggregatedFilterer, _aggregatedJoiner, new AggregatedRangeAccessor());

        private static readonly RangeFilterer<RawDataRange> _rawFilterer = new RangeFilterer<RawDataRange>(new RawDataRangeCreator(), new RawDataRangeFilteringPolicy(), new RawDataRangeAccessor());
        private static readonly RangeJoiner<RawDataRange> _rawJoiner = new RangeJoiner<RawDataRange>(new RawDataRangeCreator(), new RawDataRangeAccessor(), new Mock<ILogger<RangeJoiner<RawDataRange>>>().Object);
        private readonly RangeMerger<RawDataRange> _rawMerger = new RangeMerger<RawDataRange>(_rawFilterer, _rawJoiner, new RawDataRangeAccessor());

        [Fact]
        public void DontMergeBefore()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 50);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 50), new TimeRange(100, 200) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStart()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 100);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 200) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeOverlappingStart()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 120);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 200) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeOverlappingStartTouchingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 200);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 200) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeOverlappingAll()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 300);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 300) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStartMiddle()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(100, 150);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStartTouchingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(100, 200);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStartOverlappingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(100, 300);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 300) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeMiddleTouchingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(150, 200);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeMiddleOverlappingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(150, 250);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 250) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(200, 250);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 250) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DontMergeAfterEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(220, 250);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(220, 250) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DontMergeBetween()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(220, 280);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(220, 280), new TimeRange(300, 400) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenTouchingFirst()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(200, 280);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 280), new TimeRange(300, 400) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenTouchingSecond()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(220, 300);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(220, 400) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenTouchingBoth()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(200, 300);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 400) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenOverlappingBoth()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(150, 350);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 400) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenCoveringBoth()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(100, 400);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 400) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenCoveringBothMore()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(50, 450);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(50, 450) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeEndToEnd()
        {
            var range = new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50);
            var newRange = new AggregatedDataRange(400, 500, new List<double> { 450, 45, 500, 50 }, 50);
            var actual = _aggregatedMerger.MergeRangeWithRanges(new List<AggregatedDataRange> { range }, newRange);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(400, 600, new List<double> { 450, 45, 500, 50, 550, 55, 600, 60 }, 50) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DontMergeBeforeRaw()
        {
            var range = new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 });
            var newRange = new RawDataRange(new TimeRange(10, 50), new List<double> { 10, 1, 50, 5 });
            var actual = _rawMerger.MergeRangeWithRanges(new List<RawDataRange> { range }, newRange);
            var expected = new List<RawDataRange> { newRange, range };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStartRaw()
        {
            var range = new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 });
            var newRange = new RawDataRange(new TimeRange(10, 100), new List<double> { 10, 1, 50, 5, 100, 10 });
            var actual = _rawMerger.MergeRangeWithRanges(new List<RawDataRange> { range }, newRange);
            var expected = new List<RawDataRange> { new RawDataRange(10, 200, new List<double> { 10, 1, 50, 5, 100, 10, 150, 15, 200, 20 }) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeOverlappingStartRaw()
        {
            var range = new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 });
            var newRange = new RawDataRange(new TimeRange(10, 150), new List<double> { 10, 1, 100, 10, 150, 15 });
            var actual = _rawMerger.MergeRangeWithRanges(new List<RawDataRange> { range }, newRange);
            var expected = new List<RawDataRange> { new RawDataRange(10, 200, new List<double> { 10, 1, 100, 10, 150, 15, 200, 20 }) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStartInsideRaw()
        {
            var range = new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 });
            var newRange = new RawDataRange(new TimeRange(100, 150), new List<double> { 100, 10, 150, 15 });
            var actual = _rawMerger.MergeRangeWithRanges(new List<RawDataRange> { range }, newRange);
            var expected = new List<RawDataRange> { new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 }) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingEndInsideRaw()
        {
            var range = new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 });
            var newRange = new RawDataRange(new TimeRange(150, 200), new List<double> { 150, 15, 200, 20 });
            var actual = _rawMerger.MergeRangeWithRanges(new List<RawDataRange> { range }, newRange);
            var expected = new List<RawDataRange> { new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 }) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeOverlappingEndRaw()
        {
            var range = new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 });
            var newRange = new RawDataRange(new TimeRange(150, 250), new List<double> { 150, 15, 200, 20, 250, 25 });
            var actual = _rawMerger.MergeRangeWithRanges(new List<RawDataRange> { range }, newRange);
            var expected = new List<RawDataRange> { new RawDataRange(100, 250, new List<double> { 100, 10, 150, 15, 200, 20, 250, 25 }) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingEndRaw()
        {
            var range = new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 });
            var newRange = new RawDataRange(new TimeRange(200, 250), new List<double> { 200, 20, 250, 25 });
            var actual = _rawMerger.MergeRangeWithRanges(new List<RawDataRange> { range }, newRange);
            var expected = new List<RawDataRange> { new RawDataRange(100, 250, new List<double> { 100, 10, 150, 15, 200, 20, 250, 25 }) };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DontMergeAfterEndRaw()
        {
            var range = new RawDataRange(100, 200, new List<double> { 100, 10, 150, 15, 200, 20 });
            var newRange = new RawDataRange(new TimeRange(250, 300), new List<double> { 250, 25, 300, 30 });
            var actual = _rawMerger.MergeRangeWithRanges(new List<RawDataRange> { range }, newRange);
            var expected = new List<RawDataRange> { range, newRange };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeThreeRaw()
        {
            var one = new RawDataRange(1500112845, 1500112845, new List<double> { 1500112845, 10 });
            var two = new RawDataRange(1500112845, 1500150214, new List<double> { 1500112845, 10, 1500150210, 11 });
            var three = new RawDataRange(1500150214, 1500150215, new List<double> { 1500150215, 12 });
            
            var actual = _rawMerger.MergeRangesWithRange(two, new [] { one, three });
            var expected = new List<RawDataRange> { new RawDataRange(1500112845, 1500150215, new List<double> { 1500112845, 10, 1500150210, 11, 1500150215, 12 }) };
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
