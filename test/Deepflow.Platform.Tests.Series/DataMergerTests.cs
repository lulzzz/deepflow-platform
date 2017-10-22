using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using FluentAssertions;
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

        [Fact]
        public void DontMergeBefore()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 50);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 50), new TimeRange(100, 200) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStart()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 100);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 200) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeOverlappingStart()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 120);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 200) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeOverlappingStartTouchingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 200);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 200) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeOverlappingAll()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(10, 300);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(10, 300) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStartMiddle()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(100, 150);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStartTouchingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(100, 200);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingStartOverlappingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(100, 300);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 300) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeMiddleTouchingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(150, 200);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeMiddleOverlappingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(150, 250);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 250) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeTouchingEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(200, 250);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 250) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void DontMergeAfterEnd()
        {
            var range = new TimeRange(100, 200);
            var newRange = new TimeRange(220, 250);
            var actual = _merger.MergeRangeWithRanges(new List<TimeRange> { range }, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(220, 250) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void DontMergeBetween()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(220, 280);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(220, 280), new TimeRange(300, 400) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenTouchingFirst()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(200, 280);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 280), new TimeRange(300, 400) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenTouchingSecond()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(220, 300);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(220, 400) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenTouchingBoth()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(200, 300);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 400) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenOverlappingBoth()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(150, 350);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 400) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenCoveringBoth()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(100, 400);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(100, 400) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeBetweenCoveringBothMore()
        {
            var ranges = new List<TimeRange> { new TimeRange(100, 200), new TimeRange(300, 400) };
            var newRange = new TimeRange(50, 450);
            var actual = _merger.MergeRangeWithRanges(ranges, newRange);
            var expected = new List<TimeRange> { new TimeRange(50, 450) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void MergeEndToEnd()
        {
            var range = new AggregatedDataRange(500, 600, new List<double> { 550, 55, 600, 60 }, 50);
            var newRange = new AggregatedDataRange(400, 500, new List<double> { 450, 45, 500, 50 }, 50);
            var actual = _aggregatedMerger.MergeRangeWithRanges(new List<AggregatedDataRange> { range }, newRange);
            var expected = new List<AggregatedDataRange> { new AggregatedDataRange(400, 600, new List<double> { 450, 45, 500, 50, 550, 55, 600, 60 }, 50) };
            actual.ShouldBeEquivalentTo(expected);
        }
    }
}
