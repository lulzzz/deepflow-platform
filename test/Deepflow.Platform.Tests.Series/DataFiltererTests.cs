using System;
using System.Collections.Generic;
using System.Text;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using FluentAssertions;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    public class DataFiltererTests
    {
        private readonly DataFilterer _dataFilterer = new DataFilterer();

        [Fact]
        public void FilterSingleBefore()
        {
            var timeRange = new TimeRange(0, 100);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange>();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleTouchingStart()
        {
            var timeRange = new TimeRange(100, 200);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 200, new List<double> { 200, 20 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleOverlappingStart()
        {
            var timeRange = new TimeRange(100, 220);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 220, new List<double> { 200, 20 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleOverlappingStartMore()
        {
            var timeRange = new TimeRange(100, 250);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 250, new List<double> { 200, 20, 250, 25 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleInsideStart()
        {
            var timeRange = new TimeRange(200, 220);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 220, new List<double> { 200, 20 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleInsideStartMore()
        {
            var timeRange = new TimeRange(200, 250);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 250, new List<double> { 200, 20, 250, 25 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleMiddle()
        {
            var timeRange = new TimeRange(220, 250);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(220, 250, new List<double> { 250, 25 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleMiddleMore()
        {
            var timeRange = new TimeRange(220, 280);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(220, 280, new List<double> { 250, 25 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleInsideEnd()
        {
            var timeRange = new TimeRange(250, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(250, 300, new List<double> { 250, 25, 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleInsideEndMore()
        {
            var timeRange = new TimeRange(220, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(220, 300, new List<double> { 250, 25, 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleOverlappingEnd()
        {
            var timeRange = new TimeRange(280, 350);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(280, 300, new List<double> { 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleOverlappingEndMore()
        {
            var timeRange = new TimeRange(250, 350);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(250, 300, new List<double> { 250, 25, 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleTouchingEnd()
        {
            var timeRange = new TimeRange(300, 350);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(300, 300, new List<double> { 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleAfterEnd()
        {
            var timeRange = new TimeRange(350, 400);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange>();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleCovering()
        {
            var timeRange = new TimeRange(100, 400);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleMatching()
        {
            var timeRange = new TimeRange(200, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleMatchingPastStart()
        {
            var timeRange = new TimeRange(100, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterSingleMatchingPastEnd()
        {
            var timeRange = new TimeRange(200, 400);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterNullTimeRange()
        {
            TimeRange timeRange = null;
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            List<DataRange> expected = new List<DataRange>();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterEmptyRanges()
        {
            var timeRange = new TimeRange(350, 400);
            var ranges = new List<DataRange>();
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            var expected = new List<DataRange>();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void FilterNullRanges()
        {
            var timeRange = new TimeRange(350, 400);
            List<DataRange> ranges = null;
            var actual = _dataFilterer.FilterDataRanges(ranges, timeRange);
            List<DataRange> expected = null;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeBefore()
        {
            var timeRange = new TimeRange(100, 150);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeTouchingStart()
        {
            var timeRange = new TimeRange(100, 200);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeOverlappingStart()
        {
            var timeRange = new TimeRange(100, 250);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(250, 300, new List<double> { 250, 25, 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeInsideStartABit()
        {
            var timeRange = new TimeRange(200, 220);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(220, 300, new List<double> { 250, 25, 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeInsideStartMore()
        {
            var timeRange = new TimeRange(200, 250);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(250, 300, new List<double> { 250, 25, 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeInsideStartEvenMore()
        {
            var timeRange = new TimeRange(200, 280);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(280, 300, new List<double> { 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeInside()
        {
            var timeRange = new TimeRange(220, 280);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 220, new List<double> { 200, 20 }), new DataRange(280, 300, new List<double> { 300, 30 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeSingleMiddleStart()
        {
            var timeRange = new TimeRange(220, 220);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeSingleMiddle()
        {
            var timeRange = new TimeRange(250, 250);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeSingleMiddleEnd()
        {
            var timeRange = new TimeRange(280, 280);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeInsideEndABit()
        {
            var timeRange = new TimeRange(280, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 280, new List<double> { 200, 20, 250, 25 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeInsideEndMore()
        {
            var timeRange = new TimeRange(250, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 250, new List<double> { 200, 20, 250, 25 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeInsideEndEvenMore()
        {
            var timeRange = new TimeRange(220, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 220, new List<double> { 200, 20 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeOverlappingEnd()
        {
            var timeRange = new TimeRange(220, 350);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange> { new DataRange(200, 220, new List<double> { 200, 20 }) };
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeTouchingEnd()
        {
            var timeRange = new TimeRange(300, 350);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeAfterEnd()
        {
            var timeRange = new TimeRange(350, 400);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = ranges;
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeMatching()
        {
            var timeRange = new TimeRange(200, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange>();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeMatchingPastStart()
        {
            var timeRange = new TimeRange(100, 300);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange>();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeMatchingPastEnd()
        {
            var timeRange = new TimeRange(200, 400);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange>();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SubtractTimeRangeCovering()
        {
            var timeRange = new TimeRange(100, 400);
            var ranges = new List<DataRange> { new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 }) };
            var actual = _dataFilterer.SubtractTimeRangeFromRanges(ranges, timeRange);
            var expected = new List<DataRange>();
            actual.ShouldBeEquivalentTo(expected);
        }
    }
}