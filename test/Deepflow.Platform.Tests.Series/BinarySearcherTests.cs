using System;
using System.Collections.Generic;
using System.Text;
using Deepflow.Platform.Series;
using FluentAssertions;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    public class BinarySearcherTests
    {
        [Fact]
        public void SearchBeforeOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 0;
            var expected = 0;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchStartOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 100;
            var expected = 0;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterStartOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 150;
            var expected = 2;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchStartMiddleOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 200;
            var expected = 2;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterStartMiddleOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 250;
            var expected = 4;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchMiddleOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 300;
            var expected = 4;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterMiddleOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 350;
            var expected = 6;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchEndMiddleOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 400;
            var expected = 6;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterEndMiddleOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 450;
            var expected = 8;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchEndOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 500;
            var expected = 8;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterEndOdd()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40, 500, 50 };
            var time = 550;
            var expected = (int?)null;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }
        
        [Fact]
        public void SearchBeforeEvent()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 0;
            var expected = 0;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchStartEven()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 100;
            var expected = 0;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterStartEven()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 150;
            var expected = 2;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchStartMiddleEven()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 200;
            var expected = 2;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterStartMiddleEvent()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 250;
            var expected = 4;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchMiddleEven()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 300;
            var expected = 4;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterMiddleEven()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 350;
            var expected = 6;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchEndMiddleEven()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 400;
            var expected = 6;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterEndEven()
        {
            var data = new List<double> { 100, 10, 200, 20, 300, 30, 400, 40 };
            var time = 450;
            var expected = (int?)null;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchBeforeOne()
        {
            var data = new List<double> { 100, 10 };
            var time = 50;
            var expected = 0;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchEqualOne()
        {
            var data = new List<double> { 100, 10 };
            var time = 100;
            var expected = 0;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchAfterOne()
        {
            var data = new List<double> { 100, 10 };
            var time = 150;
            var expected = (int?)null;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void SearchEmpty()
        {
            var data = new List<double>();
            var time = 100;
            var expected = (int?)null;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }

        /*[Fact]
        public void SearchEmpty()
        {
            var data = new List<double> { 250, 25, 300, 30 };
            var time = 250;
            var expected = (int?)null;
            var actual = BinarySearcher.GetFirstIndexEqualOrGreaterTimestampBinary(data, time);
            actual.ShouldBeEquivalentTo(expected);
        }*/
    }
}
