﻿using LibraryCore.Core.ExtensionMethods;
using System.Text;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class IEnumerableExtensionMethodTest
{

    #region Any With Null Check Tests

    /// <summary>
    /// Unit test the no predicate version
    /// </summary>
    [Fact]
    public void AnyWithNullCheckTest1()
    {
        //create a new null list that we will use to check
        List<int>? ListToTestWith = null;

        //check the null list
        Assert.False(ListToTestWith.AnyWithNullCheck());

        //create a new list
        ListToTestWith = [];

        //check if the object instance has any items
        Assert.False(ListToTestWith.AnyWithNullCheck());

        //add an item to the list
        ListToTestWith.Add(1);

        //do we see the 1 number
        Assert.True(ListToTestWith.AnyWithNullCheck());

        //add another item
        ListToTestWith.Add(2);

        //should see the 2 items
        Assert.True(ListToTestWith.AnyWithNullCheck());

        //clear all the items
        ListToTestWith.Clear();

        //should resolve to false
        Assert.False(ListToTestWith.AnyWithNullCheck());
    }

    /// <summary>
    /// Unit test the version with the predicate
    /// </summary>
    [Fact]
    public void AnyWithNullCheckPredicateTest1()
    {
        //create a new null list that we will use to check
        List<int>? ListToTestWith = null;

        //should return false since we don't have an instance of an object
        Assert.False(ListToTestWith.AnyWithNullCheck(x => x == 5));

        //create an instance of the list now
        ListToTestWith = [];

        //we still don't have any items in the list
        Assert.False(ListToTestWith.AnyWithNullCheck(x => x == 5));

        //add an item now 
        ListToTestWith.Add(1);

        //we should be able to find the == 1
        Assert.True(ListToTestWith.AnyWithNullCheck(x => x == 1));

        //we don't have anything greater then 5
        Assert.False(ListToTestWith.AnyWithNullCheck(x => x > 5));

        //add 2
        ListToTestWith.Add(2);

        //should be able to find the 2
        Assert.True(ListToTestWith.AnyWithNullCheck(x => x == 2));

        //shouldn't be able to find any numbers greater then 5
        Assert.False(ListToTestWith.AnyWithNullCheck(x => x > 5));

        //clear the list
        ListToTestWith.Clear();

        //we have no items because we just cleared the list
        Assert.False(ListToTestWith.AnyWithNullCheck(x => x <= 5));
    }

    #endregion

    #region Has None

    [Fact]
    public void HasNoneIsNull()
    {
        List<int>? list = null;

        Assert.True(list.IsNullOrEmpty());
    }

    [Fact]
    public void HasNoneIsTrue()
    {
        Assert.True(new List<int>().IsNullOrEmpty());
    }

    [Fact]
    public void HasNoneIsFalse()
    {
        Assert.False(new List<int> { 1 }.IsNullOrEmpty());
    }

    [Fact]
    public void HasNoneWithPredicateIsNull()
    {
        List<int>? list = null;

        Assert.True(list.IsNullOrEmpty(x => x == 3));
    }

    [Fact]
    public void HasNoneWithPredicateTrue()
    {
        Assert.True(new List<int>().IsNullOrEmpty(x => true));
    }

    [Fact]
    public void HasNoneWithPredicateTrueWithRecords()
    {
        Assert.True(new List<int> { 1, 2, 3 }.IsNullOrEmpty(x => x == 4));
    }

    [Fact]
    public void HasNoneWithPredicateFalseWithRecords()
    {
        Assert.False(new List<int> { 1, 2, 3 }.IsNullOrEmpty(x => x == 3));
    }

    #endregion

    #region For Each

    /// <summary>
    /// Unit test the basic functionality of the foreach on ienumerable
    /// </summary>
    [Fact]
    public void ForEachTest()
    {
        IEnumerable<int> testData = new List<int> { 1, 2, 3 };

        var builder = new StringBuilder();

        testData.ForEach(x =>
        {
            builder.Append(x);
        });

        Assert.Equal("123", builder.ToString());
    }

    #endregion

    #region Empty If Null Tests

    /// <summary>
    /// Test an enumerable that is not empty. Should pass back the original enumerable
    /// </summary>
    [Fact]
    public void EmptyIfNullWithEnumerableThatIsNotNullTest1()
    {
        //original item to test
        var originalEnumerable = new List<string> { "1", "2", "3" };

        //go use the helper to check the result
        Assert.Equal(originalEnumerable, originalEnumerable.EmptyIfNull());
    }

    /// <summary>
    /// Test an enumerable that is not empty. Should pass back the original enumerable
    /// </summary>
    [Fact]
    public void EmptyIfNullWithEnumerableThatIsNullTest1()
    {
        //original item to test
        List<string>? originalEnumerable = null;

        //go grab the result. (pass in empty enumerable...because the result should be empty)
        Assert.Equal([], originalEnumerable.EmptyIfNull());
    }

    #endregion

    #region Select Async

    [Fact]
    public async Task SelectAsyncTest()
    {
        var list = new List<string> { "a", "b", "c" };

        var result = await list.SelectAsync(async x => await Task.FromResult(x + "-9876"));

        Assert.Equal(3, result.Count());
        Assert.Contains(result, x => x == "a-9876");
        Assert.Contains(result, x => x == "b-9876");
        Assert.Contains(result, x => x == "c-9876");
    }

    #endregion

    #region First Index Of

    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, null)]
    [Theory]
    public void FirstIndexOfTest(int predicate, int? expectedResult)
    {
        Assert.Equal(expectedResult, new[] { 1, 2, 3 }.FirstIndexOfElement(x => x == predicate));
    }

    #endregion

    #region With Index

    [Fact]
    public void WithIndexSimpleTypeTest()
    {
        var arrayToTestWith = new[] { "a", "b", "c" };

        //this is just to verify the data is correct. You don't need this when calling the code in a normal scenario
        int i = 0;

        foreach (var (index, value) in arrayToTestWith.WithIndex())
        {
            if (i == 0)
            {
                Assert.Equal(0, index);
                Assert.Equal("a", value);
            }
            else if (i == 1)
            {
                Assert.Equal(1, index);
                Assert.Equal("b", value);
            }
            else if (i == 2)
            {
                Assert.Equal(2, index);
                Assert.Equal("c", value);
            }
            else
            {
                throw new Exception("Shouldn't be called");
            }

            i++;
        }
    }

    public record WithIndexObject(int Id, string Text);

    [Fact]
    public void WithIndexObjectTypeTest()
    {
        var arrayToTestWith = new[] {
                new WithIndexObject(0, "A"),
                new WithIndexObject(1, "B")
            };

        //this is just to verify the data is correct. You don't need this when calling the code in a normal scenario
        int i = 0;

        foreach (var result in arrayToTestWith.WithIndex())
        {
            if (i == 0)
            {
                Assert.Equal(0, result.Index);
                Assert.Equal("A", result.Value.Text);
            }
            else if (i == 1)
            {
                Assert.Equal(1, result.Index);
                Assert.Equal("B", result.Value.Text);
            }
            else
            {
                throw new Exception("Shouldn't be called");
            }

            i++;
        }
    }

    #endregion

    #region Median

#if NET7_0_OR_GREATER

    #region Median

    [Fact]
    public void MedianWithNullSource()
    {
        List<int> lst = null!;

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => lst.Median<int, double>());
    }

    [Fact]
    public void MedianWithEmptySource()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<int>().Median<int, double>());
    }

    [InlineData(new[] { 1, 2, 3, 4 }, 2.5)] //even item
    [InlineData(new[] { 3, 2, 1 }, 2)] //odd item
    [Theory]
    public void Median_With_Ints(IEnumerable<int> source, double expectedResult)
    {
        Assert.Equal(expectedResult, source.Median<int,double>());
    }

    [InlineData(new[] { 1D, 1, 1, 5, 6, 7, 8, 9 }, 5.5)] //even item
    [InlineData(new[] { 1D, 15, 25, 5, 6, 7, 2.5, 9 }, 6.5)] //even item
    [InlineData(new[] { 3D, 2, 1 }, 2)] //odd item
    [InlineData(new[] { 3D, 25, 16 }, 16)] //odd item
    [Theory]
    public void Median_With_Doubles(IEnumerable<double> source, double expectedResult)
    {
        Assert.Equal(expectedResult, source.Median<double, double>());
    }

    [InlineData(new[] { 1D, 1, 1, 5, 6, 7, 8, 9 }, 5.5)] //even item
    [InlineData(new[] { 1D, 15, 25, 5, 6, 7, 2.5, 9 }, 6.5)] //even item
    [InlineData(new[] { 3D, 2, 1 }, 2)] //odd item
    [InlineData(new[] { 3D, 25, 16 }, 16)] //odd item
    [Theory]
    public void Median_With_DoubleToFloat(IEnumerable<double> source, float expectedResult)
    {
        Assert.Equal(expectedResult, source.Median<double, float>());
    }

    [InlineData(new[] { 1F, 1, 1, 5, 6, 7, 8, 9 }, 5.5)] //even item
    [InlineData(new[] { 1F, 15, 25, 5, 6, 7, 2.5F, 9 }, 6.5)] //even item
    [InlineData(new[] { 3F, 2, 1 }, 2)] //odd item
    [InlineData(new[] { 3F, 25, 16 }, 16)] //odd item
    [Theory]
    public void Median_With_Floats(IEnumerable<float> source, double expectedResult)
    {
        Assert.Equal(expectedResult, source.Median<float, double>());
    }

    [InlineData(new[] { 1F, 1, 1, 5, 6, 7, 8, 9 }, 5.5)] //even item
    [InlineData(new[] { 1F, 15, 25, 5, 6, 7, 2.5F, 9 }, 6.5)] //even item
    [InlineData(new[] { 3F, 2, 1 }, 2)] //odd item
    [InlineData(new[] { 3F, 25, 16 }, 16)] //odd item
    [Theory]
    public void Median_With_Float_Float(IEnumerable<float> source, float expectedResult)
    {
        Assert.Equal(expectedResult, source.Median<float, float>());
    }

    #endregion

#endif

    #endregion

    #region Mode

    [Fact]
    public void ModeWithNullSource()
    {
        Assert.Throws<ArgumentException>(() => ((List<int>)null!).Mode());
    }

    [Fact]
    public void ModeWithEmptySource()
    {
        Assert.Throws<ArgumentException>(() => Array.Empty<int>().Mode());
    }

    [InlineData(new[] { 1D, 2, 2, 2, 5, 3, 3, 3, 6, 7, 8, 9 }, new[] { 2D, 3 })] //even item
    [InlineData(new[] { 1D, 2, 3, 4, 5, 2 }, new[] { 2D })] //even item
    [Theory]
    public void ModeMultipleChecks(IEnumerable<double> source, IEnumerable<double> expectedResult)
    {
        var result = source.Mode();

        foreach (var expected in expectedResult)
        {
            Assert.Contains(result, x => x == expected);
        }
    }

    #endregion

    #region Partitioning

    [Fact]
    public void PartitioningWithNullSource()
    {
        Assert.Throws<ArgumentNullException>(() => ((List<int>)null!).Partition(x => x % 2 == 0));
    }

    [Fact]
    public void PartitioningWithNullPredicate()
    {
        Assert.Throws<ArgumentNullException>(() => Array.Empty<int>().Partition(null!));
    }

    [Fact]
    public void PartitioningMultipleChecks()
    {
        var result = new[] { 1, 2, 3, 4, 6 }.Partition(x => x % 2 == 0);

        Assert.Equal(3, result.True.Count());
        Assert.Equal(2, result.False.Count());

        Assert.Contains(result.True, x => x == 2);
        Assert.Contains(result.True, x => x == 4);
        Assert.Contains(result.True, x => x == 6);

        Assert.Contains(result.False, x => x == 1);
        Assert.Contains(result.False, x => x == 3);
    }

    #endregion

}
