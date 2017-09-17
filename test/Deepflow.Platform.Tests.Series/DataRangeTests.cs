using Deepflow.Platform.Abstractions.Series;
using FluentAssertions;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    public class DataRangeTests
    {
        [Theory]
        [InlineData(100, 200, 300, 400)]
        [InlineData(300, 400, 100, 200)]
        public void TestDoesntTouch(int minOne, int maxOne, int minTwo, int maxTwo)
        {
            var one = new AggregatedDataRange(minOne, maxOne, 50);
            var two = new AggregatedDataRange(minTwo, maxTwo, 50);
            one.TimeRange.Touches(two.TimeRange).Should().BeFalse();
        }

        [Theory]
        [InlineData(100, 200, 200, 300)]
        [InlineData(100, 250, 200, 300)]
        [InlineData(100, 300, 200, 300)]
        [InlineData(200, 300, 200, 300)]
        [InlineData(250, 270, 200, 300)]
        [InlineData(250, 350, 200, 300)]
        [InlineData(300, 350, 200, 300)]
        public void TestTouches(int minOne, int maxOne, int minTwo, int maxTwo)
        {
            var one = new AggregatedDataRange(minOne, maxOne, 50);
            var two = new AggregatedDataRange(minTwo, maxTwo, 50);
            one.TimeRange.Touches(two.TimeRange).Should().BeTrue();
        }
    }
}
