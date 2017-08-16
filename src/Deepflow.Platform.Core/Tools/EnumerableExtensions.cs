using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Core.Tools
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            T[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new T[size];

                bucket[count++] = item;

                if (count != size)
                    continue;

                yield return bucket.Select(x => x);

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
                yield return bucket.Take(count);
        }

        public static IEnumerable<T> GetNth<T>(this IEnumerable<T> source, int n)
        {
            using (var enumerator = source.GetEnumerator())
            {
                var i = 0;
                while (enumerator.MoveNext())
                {
                    if (i % n == 0)
                    {
                        yield return enumerator.Current;
                    }
                    i++;
                }
            }
        }

        public static IEnumerable<Datum> GetData(this IEnumerable<DataRange> source)
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
                            if (i % 2 == 0)
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

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item)
        {
            return source.Concat(new [] { item });
        }

        public static IEnumerable<T> JoinInsert<T>(this IEnumerable<T> source, T itemToInsert, Func<T, T, InsertPlacement> placer, Func<T, T, T> joiner)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (itemToInsert == null)
                {
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                    yield break;
                }

                bool inserted = false;
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    if (item == null)
                    {
                        yield return item;
                        continue;
                    }

                    if (inserted)
                    {
                        yield return item;
                        continue;
                    }

                    var placement = placer(itemToInsert, item);
                    if (placement == InsertPlacement.Before)
                    {
                        inserted = true;
                        yield return itemToInsert;
                        yield return item;
                    }
                    else if (placement == InsertPlacement.Equal)
                    {
                        inserted = true;
                        yield return joiner(item, itemToInsert);
                    }
                    else if (placement == InsertPlacement.After)
                    {
                        yield return item;
                    }
                }

                if (!inserted)
                {
                    yield return itemToInsert;
                }
            }
        }
    }

    public enum InsertPlacement
    {
        Before,
        Equal,
        After
    }
}
