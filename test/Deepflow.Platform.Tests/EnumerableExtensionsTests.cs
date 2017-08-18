using Deepflow.Platform.Core.Tools;
using FluentAssertions;
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
    }
}
