/*
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using Deepflow.Platform.Series.Providers;
using FluentAssertions;
using Xunit;

namespace Deepflow.Platform.Tests.Series
{
    public class DeterministicRandomDataProviderTests
    {
        [Fact]
        public void Test()
        {
            var name = "Test";
            var aggregations = new [] { 400, 200 };
            var aggregationLevel = 200;
            var timeRange = new TimeRange(1000, 1400);
            var values = new Dictionary<int, Dictionary<long, double>>
            {
                { 400, new Dictionary<long, double> { { 800, 80 }, { 1200, 120 }, { 1600, 160 } } },
                { 200, new Dictionary<long, double> { { 800, 8 }, { 1000, 10 }, { 1200, 12 }, { 1400, 14 }, { 1600, 16 } } }
            };
            var valueGenerator = new MockValueGenerator(values);
            var generator = new ReverseAverageGenerator(new DataFilterer(), valueGenerator, new DataMerger(new DataFilterer(), new DataJoiner()));
            var actual = generator.GenerateReverseAverage(name, timeRange, aggregations, aggregationLevel, 500, 1500);
            var expected = new RawDataRange(timeRange, new List<double> { 1000, 10, 1200, 12, 1400, 14 });
            actual.ShouldBeEquivalentTo(expected);
        }

        private class MockValueGenerator : IValueGenerator
        {
            private readonly Dictionary<int, Dictionary<long, double>> _values;

            public MockValueGenerator(Dictionary<int, Dictionary<long, double>> values)
            {
                _values = values;
            }

            public double GenerateValue(string name, int aggregationSeconds, long timeSeconds, double minValue, double maxValue)
            {
                return _values[aggregationSeconds][timeSeconds];
            }
        }
    }
}
*/
