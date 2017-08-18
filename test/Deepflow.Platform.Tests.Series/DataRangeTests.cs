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
            var one = new DataRange(minOne, maxOne, null);
            var two = new DataRange(minTwo, maxTwo, null);
            one.Touches(two).Should().BeFalse();
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
            var one = new DataRange(minOne, maxOne, null);
            var two = new DataRange(minTwo, maxTwo, null);
            one.Touches(two).Should().BeTrue();
        }
    }
}
