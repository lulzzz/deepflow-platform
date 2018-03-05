using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    public class DataJoinerTests
    {
        private readonly RangeJoiner<AggregatedDataRange> _rangeJoiner = new RangeJoiner<AggregatedDataRange>(new AggregatedRangeCreator(), new AggregatedRangeAccessor(), new Mock<ILogger<RangeJoiner<AggregatedDataRange>>>().Object);

        [Fact]
        public void AddToEmpty()
        {
            var insert = new AggregatedDataRange(100, 200, new List<double> { 150, 16, 200, 21 }, 50);
            var actual = _rangeJoiner.JoinDataRangesToDataRanges(new List<AggregatedDataRange>(), new List<AggregatedDataRange> { insert });
            var expected = new List<AggregatedDataRange> { insert };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void JoinBefore()
        {
            var insert = new AggregatedDataRange(100, 200, new List<double> { 150, 16, 200, 21 }, 50);
            var old = new AggregatedDataRange(300, 400, new List<double> { 350, 35, 400, 40 }, 50);
            TestJoinTwoDataRanges(insert, old, insert, old);
        }

        [Fact]
        public void JoinTouchingStartOutside()
        {
            var insert = new AggregatedDataRange(100, 200, new List<double> { 150, 16, 200, 21 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(100, 300, new List<double> { 150, 16, 200, 21, 250, 25, 300, 30 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinOverlappingStart()
        {
            var insert = new AggregatedDataRange(100, 250, new List<double> { 150, 16, 200, 21, 250, 26 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(100, 300, new List<double> { 150, 16, 200, 21, 250, 26, 300, 30 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinTouchingStartInside()
        {
            var insert = new AggregatedDataRange(200, 250, new List<double> { 250, 26 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(200, 300, new List<double> { 250, 26, 300, 30 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinInside()
        {
            var insert = new AggregatedDataRange(240, 260, new List<double> { 260, 27 }, 20);
            var old = new AggregatedDataRange(200, 300, new List<double> { 220, 22, 240, 24, 260, 26, 280, 28, 300, 30 }, 20);
            var expected = new AggregatedDataRange(200, 300, new List<double> { 220, 22, 240, 24, 260, 27, 280, 28, 300, 30 }, 20);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinTouchingEndInside()
        {
            var insert = new AggregatedDataRange(250, 300, new List<double> { 300, 31 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 31 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinOverlappingEnd()
        {
            var insert = new AggregatedDataRange(250, 350, new List<double> { 300, 31, 350, 36 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(200, 350, new List<double> { 250, 25, 300, 31, 350, 36 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinTouchingEndOutside()
        {
            var insert = new AggregatedDataRange(300, 350, new List<double> { 350, 36 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(200, 350, new List<double> { 250, 25, 300, 30, 350, 36 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinAfter()
        {
            var insert = new AggregatedDataRange(400, 500, new List<double> { 450, 46, 500, 51 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            TestJoinTwoDataRanges(insert, old, insert, old);
        }

        [Fact]
        public void JoinCoveringTouchingEnd()
        {
            var insert = new AggregatedDataRange(100, 300, new List<double> { 150, 16, 200, 21, 250, 26, 300, 31 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(100, 300, new List<double> { 150, 16, 200, 21, 250, 26, 300, 31 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinCoveringTouchingStart()
        {
            var insert = new AggregatedDataRange(200, 400, new List<double> { 250, 26, 300, 31, 350, 36, 400, 41 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(200, 400, new List<double> { 250, 26, 300, 31, 350, 36, 400, 41 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinCoveringOver()
        {
            var insert = new AggregatedDataRange(100, 400, new List<double> { 150, 16, 200, 21, 250, 26, 300, 31, 350, 36, 400, 41 }, 50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var expected = new AggregatedDataRange(100, 400, new List<double> { 150, 16, 200, 21, 250, 26, 300, 31, 350, 36, 400, 41 }, 50);
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinNullInsert()
        {
            AggregatedDataRange insert = null;
            var old = new AggregatedDataRange(200, 300, new List<double> {250, 25, 300, 30 }, 50);
            TestJoinTwoDataRanges(insert, old, old);
        }

        [Fact]
        public void JoinNullOld()
        {
            var insert = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            List<AggregatedDataRange> old = null;
            TestJoinTwoDataRanges(insert, old, insert);
        }

        [Fact]
        public void JoinNullBoth()
        {
            AggregatedDataRange insert = null;
            AggregatedDataRange old = null;
            TestJoinTwoDataRanges(insert, old, null);
        }

        [Fact]
        public void JoinNullInsertData()
        {
            AggregatedDataRange insert = new AggregatedDataRange(200, 300, new List<double>(),  50);
            var old = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            TestJoinTwoDataRanges(insert, old, old);
        }

        [Fact]
        public void JoinNullInsertOld()
        {
            var insert = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            AggregatedDataRange old = new AggregatedDataRange(200, 300, new List<double>(), 50);
            TestJoinTwoDataRanges(insert, old, insert);
        }

        [Fact]
        public void JoinBetweenTwoRanges()
        {
            var before = new AggregatedDataRange(200, 300, new List<double> { 250, 25, 300, 30 }, 50);
            var insert = new AggregatedDataRange(300, 400, new List<double> { 350, 35, 400, 40 }, 50);
            var after = new AggregatedDataRange(400, 500, new List<double> { 450, 45, 500, 50 }, 50);

            var expected = new AggregatedDataRange(200, 500, new List<double> { 250, 25, 300, 30, 350, 35, 400, 40, 450, 45, 500, 50 }, 50);

            var actual = _rangeJoiner.JoinDataRangeToDataRanges(new List<AggregatedDataRange> { before, after }, insert).ToList();
            actual.Should().BeEquivalentTo(new List<AggregatedDataRange> { expected });
        }

        private void TestJoinTwoDataRanges(AggregatedDataRange insert, AggregatedDataRange old, AggregatedDataRange expected)
        {
            var ranges = new List<AggregatedDataRange> { old };
            var actual = _rangeJoiner.JoinDataRangeToDataRanges(ranges, insert).ToList();
            actual.Should().BeEquivalentTo(new List<AggregatedDataRange> { expected });
        }

        private void TestJoinTwoDataRanges(AggregatedDataRange insert, AggregatedDataRange old, AggregatedDataRange expectedOne, AggregatedDataRange expectedTwo)
        {
            var ranges = new List<AggregatedDataRange> { old };
            var actual = _rangeJoiner.JoinDataRangeToDataRanges(ranges, insert).ToList();
            actual.Should().BeEquivalentTo(new List<AggregatedDataRange> { expectedOne, expectedTwo });
        }

        private void TestJoinTwoDataRanges(AggregatedDataRange insert, List<AggregatedDataRange> old, AggregatedDataRange expected)
        {
            var actual = _rangeJoiner.JoinDataRangeToDataRanges(old, insert).ToList();
            actual.Should().BeEquivalentTo(new List<AggregatedDataRange> { expected });
        }
    }
}
