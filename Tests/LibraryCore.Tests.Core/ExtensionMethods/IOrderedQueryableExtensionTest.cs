using LibraryCore.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class IOrderedQueryableExtensionTest
{
    private record DummyRecord(int Id, string Text)
    {
        public static IEnumerable<DummyRecord> CreateDummyList(int howMany) => Enumerable.Range(0, howMany).Select(x => new DummyRecord(x, x.ToString()));

        public static IOrderedQueryable<DummyRecord> CreateDummyQueryable(int howMany) =>
            CreateDummyList(howMany)
            .AsQueryable()
            .OrderBy(x => x.Id);
    }

    [Fact]
    public void PaginateThrowsOnCurrentPageZero()
    {
        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            DummyRecord.CreateDummyQueryable(5)
                              .PaginateResults(0, 10)
                              .ToArray();
        });
    }

    /// <summary>
    /// Unit test for pagination in linq to objects
    /// </summary>
    [Fact]
    public void PaginateForLinqToObjectsTest1()
    {
        //grab the paged data
        var pagedData = DummyRecord.CreateDummyQueryable(100)
                                .PaginateResults(2, 10)
                                .ToArray();

        //go check the results
        Assert.Equal(10, pagedData.Length);
        Assert.Equal(10, pagedData[0].Id);
        Assert.Equal(11, pagedData[1].Id);
        Assert.Equal(12, pagedData[2].Id);
    }
}
