using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static IEnumerable<Datum> GetData(this DataRange source)
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

                        T previous = joiner(item, itemToInsert);
                        
                        var hasNext = enumerator.MoveNext();
                        if (!hasNext)
                        {
                            yield return previous;
                            continue;
                        }

                        var next = enumerator.Current;
                        var nextPlacement = placer(previous, next);
                        if (nextPlacement == InsertPlacement.Equal)
                        {
                            yield return joiner(previous, next);
                        }
                        else
                        {
                            yield return previous;
                            yield return next;
                        }
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

        private static readonly Random shuffleRandom = new Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = shuffleRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, int parallelism, Func<T, Task> body)
        {
            var partitions = Partitioner.Create(source).GetPartitions(parallelism);

            var tasks = partitions.Select(partition => Task.Run(async () =>
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await body(partition.Current);
                    }
                }
            }));

            return Task.WhenAll(tasks);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
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
