using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using FluentAssertions;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    public class DataJoinerTests
    {
        private readonly DataJoiner _dataJoiner = new DataJoiner();

        [Fact]
        public void JoinBefore()
        {
            var insert = new DataRange(100, 200, new List<double> { 100, 11, 150, 16, 200, 21 });
            var old = new DataRange(300, 400, new List<double> { 300, 30, 350, 35, 400, 40 });
            TestJoinTwoDataRanges(insert, old, insert, old);
        }

        [Fact]
        public void JoinTouchingStartOutside()
        {
            var insert = new DataRange(100, 200, new List<double> { 100, 11, 150, 16, 200, 21 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(100, 300, new List<double> { 100, 11, 150, 16, 200, 21, 250, 25, 300, 30 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinOverlappingStart()
        {
            var insert = new DataRange(100, 250, new List<double> { 100, 11, 150, 16, 200, 21, 250, 26 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(100, 300, new List<double> { 100, 11, 150, 16, 200, 21, 250, 26, 300, 30 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinTouchingStartInside()
        {
            var insert = new DataRange(200, 250, new List<double> { 200, 21, 250, 26 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(200, 300, new List<double> { 200, 21, 250, 26, 300, 30 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinInside()
        {
            var insert = new DataRange(240, 260, new List<double> { 240, 25, 260, 27 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 220, 22, 240, 24, 260, 26, 280, 28, 300, 30 });
            var expected = new DataRange(200, 300, new List<double> { 200, 20, 220, 22, 240, 25, 260, 27, 280, 28, 300, 30 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinTouchingEndInside()
        {
            var insert = new DataRange(250, 300, new List<double> { 250, 26, 300, 31 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(200, 300, new List<double> { 200, 20, 250, 26, 300, 31 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinOverlappingEnd()
        {
            var insert = new DataRange(250, 350, new List<double> { 250, 26, 300, 31, 350, 36 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(200, 350, new List<double> { 200, 20, 250, 26, 300, 31, 350, 36 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinTouchingEndOutside()
        {
            var insert = new DataRange(300, 350, new List<double> { 300, 31, 350, 36 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(200, 350, new List<double> { 200, 20, 250, 25, 300, 31, 350, 36 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinAfter()
        {
            var insert = new DataRange(400, 500, new List<double> { 400, 41, 450, 46, 500, 51 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            TestJoinTwoDataRanges(insert, old, insert, old);
        }

        [Fact]
        public void JoinCoveringTouchingEnd()
        {
            var insert = new DataRange(100, 300, new List<double> { 100, 11, 150, 16, 200, 21, 250, 26, 300, 31 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(100, 300, new List<double> { 100, 11, 150, 16, 200, 21, 250, 26, 300, 31 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinCoveringTouchingStart()
        {
            var insert = new DataRange(200, 400, new List<double> { 200, 21, 250, 26, 300, 31, 350, 36, 400, 41 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(200, 400, new List<double> { 200, 21, 250, 26, 300, 31, 350, 36, 400, 41 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinCoveringOver()
        {
            var insert = new DataRange(100, 400, new List<double> { 100, 11, 150, 16, 200, 21, 250, 26, 300, 31, 350, 36, 400, 41 });
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            var expected = new DataRange(100, 400, new List<double> { 100, 11, 150, 16, 200, 21, 250, 26, 300, 31, 350, 36, 400, 41 });
            TestJoinTwoDataRanges(insert, old, expected);
        }

        [Fact]
        public void JoinNullInsert()
        {
            DataRange insert = null;
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            TestJoinTwoDataRanges(insert, old, old);
        }

        [Fact]
        public void JoinNullOld()
        {
            var insert = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            List<DataRange> old = null;
            TestJoinTwoDataRanges(insert, old, insert);
        }

        [Fact]
        public void JoinNullBoth()
        {
            DataRange insert = null;
            DataRange old = null;
            TestJoinTwoDataRanges(insert, old, null);
        }

        [Fact]
        public void JoinNullInsertData()
        {
            DataRange insert = new DataRange(200, 300);
            var old = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            TestJoinTwoDataRanges(insert, old, old);
        }

        [Fact]
        public void JoinNullInsertOld()
        {
            var insert = new DataRange(200, 300, new List<double> { 200, 20, 250, 25, 300, 30 });
            DataRange old = new DataRange(200, 300);
            TestJoinTwoDataRanges(insert, old, insert);
        }

        private void TestJoinTwoDataRanges(DataRange insert, DataRange old, DataRange expected)
        {
            var ranges = new List<DataRange> { old };
            var actual = _dataJoiner.JoinDataRangeToDataRanges(ranges, insert).ToList();
            actual.ShouldBeEquivalentTo(new List<DataRange> { expected });
        }

        private void TestJoinTwoDataRanges(DataRange insert, DataRange old, DataRange expectedOne, DataRange expectedTwo)
        {
            var ranges = new List<DataRange> { old };
            var actual = _dataJoiner.JoinDataRangeToDataRanges(ranges, insert).ToList();
            actual.ShouldBeEquivalentTo(new List<DataRange> { expectedOne, expectedTwo });
        }

        private void TestJoinTwoDataRanges(DataRange insert, List<DataRange> old, DataRange expected)
        {
            var actual = _dataJoiner.JoinDataRangeToDataRanges(old, insert).ToList();
            actual.ShouldBeEquivalentTo(new List<DataRange> { expected });
        }
    }
}
