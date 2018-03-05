using System.Collections.Generic;
using Deepflow.Platform.Core.Tools;
using FluentAssertions;
using FluentAssertions.Common;
using Xunit;

namespace Deepflow.Platform.Tests.Core
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void JoinInsertBefore()
        {
            var item = 3;
            var items = new [] { 4, 6, 8 };
            var actual = items.JoinInsert(item, PlaceItem, JoinItem);
            var expected = new[] { 3, 4, 6, 8 };
            actual.Should().Equal(expected);
        }

        [Fact]
        public void JoinInsertStart()
        {
            var item = 4;
            var items = new[] { 4, 6, 8 };
            var actual = items.JoinInsert(item, PlaceItem, JoinItem);
            var expected = new[] { 4, 6, 8 };
            actual.Should().Equal(expected);
        }

        [Fact]
        public void JoinInsertStartMiddle()
        {
            var item = 5;
            var items = new[] { 4, 6, 8 };
            var actual = items.JoinInsert(item, PlaceItem, JoinItem);
            var expected = new[] { 4, 5, 6, 8 };
            actual.Should().Equal(expected);
        }

        [Fact]
        public void JoinInsertMiddle()
        {
            var item = 6;
            var items = new[] { 4, 6, 8 };
            var actual = items.JoinInsert(item, PlaceItem, JoinItem);
            var expected = new[] { 4, 6, 8 };
            actual.Should().Equal(expected);
        }

        [Fact]
        public void JoinInsertMiddleEnd()
        {
            var item = 7;
            var items = new[] { 4, 6, 8 };
            var actual = items.JoinInsert(item, PlaceItem, JoinItem);
            var expected = new[] { 4, 6, 7, 8 };
            actual.Should().Equal(expected);
        }

        [Fact]
        public void JoinInsertEnd()
        {
            var item = 8;
            var items = new[] { 4, 6, 8 };
            var actual = items.JoinInsert(item, PlaceItem, JoinItem);
            var expected = new[] { 4, 6, 8 };
            actual.Should().Equal(expected);
        }

        [Fact]
        public void JoinInsertAfter()
        {
            var item = 9;
            var items = new[] { 4, 6, 8 };
            var actual = items.JoinInsert(item, PlaceItem, JoinItem);
            var expected = new[] { 4, 6, 8, 9 };
            actual.Should().Equal(expected);
        }
        

        private InsertPlacement PlaceItem(int itemToInsert, int item)
        {
            if (itemToInsert < item)
            {
                return InsertPlacement.Before;
            }

            if (itemToInsert > item)
            {
                return InsertPlacement.After;
            }

            if (itemToInsert == item)
            {
                return InsertPlacement.Equal;
            }

            return default(InsertPlacement);
        }

        private int JoinItem(int one, int two)
        {
            return one;
        }
        
        [Fact]
        public void MergeManyOneList()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>> { new List<int> { 1, 2, 3 } };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 1, 2, 3 };
            actual.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void MergeManyTwoLists()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 }
            };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 1, 4, 2, 5, 3, 6 };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeManyThreeLists()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 },
                new List<int> { 7, 8, 9 }
            };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 1, 4, 7, 2, 5, 8, 3, 6, 9 };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeManyThreeListsFirstShorter()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>>
            {
                new List<int> { 1 },
                new List<int> { 4, 5, 6 },
                new List<int> { 7, 8, 9 }
            };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 1, 4, 7, 5, 8, 6, 9 };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeManyThreeListsSecondShorter()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4 },
                new List<int> { 7, 8, 9 }
            };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 1, 4, 7, 2, 8, 3, 9 };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeManyThreeListsThirdShorter()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 },
                new List<int> { 7 }
            };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 1, 4, 7, 2, 5, 3, 6 };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeManyThreeListsFirstEmpty()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>>
            {
                new List<int> { },
                new List<int> { 4, 5, 6 },
                new List<int> { 7, 8, 9 }
            };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 4, 7, 5, 8, 6, 9 };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeManyThreeListsSecondEmpty()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { },
                new List<int> { 7, 8, 9 }
            };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 1, 7, 2, 8, 3, 9 };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void MergeManyThreeListsThirdEmpty()
        {
            IEnumerable<IEnumerable<int>> enumerables = new List<IEnumerable<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 },
                new List<int> { }
            };
            var actual = enumerables.MergeMany();
            var expected = new List<int> { 1, 4, 2, 5, 3, 6 };
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
