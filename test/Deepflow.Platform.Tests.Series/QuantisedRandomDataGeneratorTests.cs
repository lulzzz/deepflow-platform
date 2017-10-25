using System;
using System.Collections.Generic;
using System.Text;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series.Providers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    public class QuantisedRandomDataGeneratorTests
    {
        private readonly QuantisedRandomDataGenerator _generator = new QuantisedRandomDataGenerator();

        [Fact]
        public void TestOneMultipleOne()
        {
            var timeRange = new TimeRange(100, 150);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 50);
            var expectedTimes = new[] { 150 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOneMultipleTwo()
        {
            var timeRange = new TimeRange(100, 200);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 50);
            var expectedTimes = new[] { 150, 200 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOneMultipleThree()
        {
            var timeRange = new TimeRange(100, 250);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 50);
            var expectedTimes = new[] { 150, 200, 250 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOneMultipleFour()
        {
            var timeRange = new TimeRange(100, 300);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 50);
            var expectedTimes = new[] { 150, 200, 250, 300 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOneMultipleFive()
        {
            var timeRange = new TimeRange(100, 350);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 50);
            var expectedTimes = new[] { 150, 200, 250, 300, 350 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOneMultipleSix()
        {
            var timeRange = new TimeRange(100, 400);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 50);
            var expectedTimes = new[] { 150, 200, 250, 300, 350, 400 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOneMultipleSeven()
        {
            var timeRange = new TimeRange(100, 450);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 50);
            var expectedTimes = new[] { 150, 200, 250, 300, 350, 400, 450 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOneMultipleEight()
        {
            var timeRange = new TimeRange(100, 500);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 50);
            var expectedTimes = new[] { 150, 200, 250, 300, 350, 400, 450, 500 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestTwoMultiplesOne()
        {
            var timeRange = new TimeRange(100, 150);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 100);
            var expectedTimes = new[] { 150 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestTwoMultiplesTwo()
        {
            var timeRange = new TimeRange(100, 200);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 100);
            var expectedTimes = new[] { 150, 200 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestTwoMultiplesThree()
        {
            var timeRange = new TimeRange(100, 250);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 100);
            var expectedTimes = new[] { 150, 200, 250 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestTwoMultiplesFour()
        {
            var timeRange = new TimeRange(100, 300);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 100);
            var expectedTimes = new[] { 150, 200, 250, 300 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestTwoMultiplesFive()
        {
            var timeRange = new TimeRange(100, 350);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 100);
            var expectedTimes = new[] { 150, 200, 250, 300, 350 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestTwoMultiplesSix()
        {
            var timeRange = new TimeRange(100, 400);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 100);
            var expectedTimes = new[] { 150, 200, 250, 300, 350, 400 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestTwoMultiplesSeven()
        {
            var timeRange = new TimeRange(100, 450);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 100);
            var expectedTimes = new[] { 150, 200, 250, 300, 350, 400, 450 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestTwoMultiplesEight()
        {
            var timeRange = new TimeRange(100, 500);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 100);
            var expectedTimes = new[] { 150, 200, 250, 300, 350, 400, 450, 500 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestThreeMultiplesOne()
        {
            var timeRange = new TimeRange(200, 250);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 200);
            var expectedTimes = new[] { 250 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestThreeMultiplesTwo()
        {
            var timeRange = new TimeRange(200, 300);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 200);
            var expectedTimes = new[] { 250, 300 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestThreeMultiplesThree()
        {
            var timeRange = new TimeRange(200, 350);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 200);
            var expectedTimes = new[] { 250, 300, 350 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestThreeMultiplesFour()
        {
            var timeRange = new TimeRange(200, 400);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 200);
            var expectedTimes = new[] { 250, 300, 350, 400 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestThreeMultiplesFive()
        {
            var timeRange = new TimeRange(200, 450);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 200);
            var expectedTimes = new[] { 250, 300, 350, 400, 450 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestThreeMultiplesSix()
        {
            var timeRange = new TimeRange(200, 500);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 200);
            var expectedTimes = new[] { 250, 300, 350, 400, 450, 500 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestThreeMultiplesSeven()
        {
            var timeRange = new TimeRange(200, 550);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 200);
            var expectedTimes = new[] { 250, 300, 350, 400, 450, 500, 550 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestThreeMultiplesEight()
        {
            var timeRange = new TimeRange(200, 600);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 200);
            var expectedTimes = new[] { 250, 300, 350, 400, 450, 500, 550, 600 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOddMultiplesOne()
        {
            var timeRange = new TimeRange(200, 250);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 150);
            var expectedTimes = new[] { 250 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOddMultiplesTwo()
        {
            var timeRange = new TimeRange(200, 300);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 150);
            var expectedTimes = new[] { 250, 300 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOddMultiplesOdd()
        {
            var timeRange = new TimeRange(200, 350);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 150);
            var expectedTimes = new[] { 250, 300, 350 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOddMultiplesFour()
        {
            var timeRange = new TimeRange(200, 400);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 150);
            var expectedTimes = new[] { 250, 300, 350, 400 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOddMultiplesFive()
        {
            var timeRange = new TimeRange(200, 450);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 150);
            var expectedTimes = new[] { 250, 300, 350, 400, 450 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOddMultiplesSix()
        {
            var timeRange = new TimeRange(200, 500);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 150);
            var expectedTimes = new[] { 250, 300, 350, 400, 450, 500 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOddMultiplesSeven()
        {
            var timeRange = new TimeRange(200, 550);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 150);
            var expectedTimes = new[] { 250, 300, 350, 400, 450, 500, 550 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        [Fact]
        public void TestOddMultiplesEight()
        {
            var timeRange = new TimeRange(200, 600);
            var actual = _generator.GenerateData("test", timeRange, 1000, 2000, 500, 50, 150);
            var expectedTimes = new[] { 250, 300, 350, 400, 450, 500, 550, 600 };

            actual.TimeRange.ShouldBeEquivalentTo(timeRange);
            AssertData(actual.Data, expectedTimes);
        }

        private void AssertData(List<double> data, int[] expectedTimes)
        {
            Assert.Equal(expectedTimes.Length * 2, data.Count);

            for (int i = 0; i < expectedTimes.Length; i++)
            {
                Assert.Equal(expectedTimes[i], data[i * 2]);
                Assert.NotEqual(0, data[i * 2 + 1]);
            }
        }
    }
}
