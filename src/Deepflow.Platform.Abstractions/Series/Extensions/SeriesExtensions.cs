using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series.Extensions
{
    public static class SeriesExtensions
    {
        public static IEnumerable<Datum> GetData(this List<double> source)
        {
            var datum = new Datum();

            for (int i = 0; i < source.Count / 2; i++)
            {
                datum.Time = source[i * 2];
                datum.Value = source[i * 2 + 1];
                yield return datum;
            }
        }

        public static IEnumerable<Datum> GetData(this IEnumerable<RawDataRange> source)
        {
            var i = 0;
            var datum = new Datum();

            using (var rangeEnumerator = source.GetEnumerator())
            {
                while (rangeEnumerator.MoveNext())
                {
                    using (var dataEnumerator = (rangeEnumerator.Current.Data as IEnumerable<double>).GetEnumerator())
                    {
                        while (dataEnumerator.MoveNext())
                        {
                            if (i % 2 == 1)
                            {
                                datum.Value = dataEnumerator.Current;
                                yield return datum;
                            }
                            else
                            {
                                datum.Time = dataEnumerator.Current;
                            }
                            i++;
                        }
                    }
                }
            }
        }

        public static IEnumerable<Datum> GetData(this RawDataRange source)
        {
            var i = 0;
            var datum = new Datum();

            using (var dataEnumerator = source.Data.GetEnumerator())
            {
                while (dataEnumerator.MoveNext())
                {
                    if (i % 2 == 1)
                    {
                        datum.Value = dataEnumerator.Current;
                        yield return datum;
                    }
                    else
                    {
                        datum.Time = dataEnumerator.Current;
                    }
                    i++;
                }
            }
        }

        public static IEnumerable<Datum> GetData(this IEnumerable<AggregatedDataRange> source)
        {
            var i = 0;
            var datum = new Datum();

            using (var rangeEnumerator = source.GetEnumerator())
            {
                while (rangeEnumerator.MoveNext())
                {
                    using (var dataEnumerator = (rangeEnumerator.Current.Data as IEnumerable<double>).GetEnumerator())
                    {
                        while (dataEnumerator.MoveNext())
                        {
                            if (i % 2 == 1)
                            {
                                datum.Value = dataEnumerator.Current;
                                yield return datum;
                            }
                            else
                            {
                                datum.Time = dataEnumerator.Current;
                            }
                            i++;
                        }
                    }
                }
            }
        }

        public static IEnumerable<Datum> GetData(this AggregatedDataRange source)
        {
            var i = 0;
            var datum = new Datum();

            using (var dataEnumerator = source.Data.GetEnumerator())
            {
                while (dataEnumerator.MoveNext())
                {
                    if (i % 2 == 1)
                    {
                        datum.Value = dataEnumerator.Current;
                        yield return datum;
                    }
                    else
                    {
                        datum.Time = dataEnumerator.Current;
                    }
                    i++;
                }
            }
        }
    }
}
