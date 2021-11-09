using LibraryCore.Core.Paging;
using Xunit;

namespace LibraryCore.Tests.Core.Paging;

public class PaginationTest
{
    /// <summary>
    /// Test dataset paging. How many pages for a given dataset
    /// </summary>
    [InlineData(5, 5, 1)]
    [InlineData(6, 3, 2)]
    [InlineData(6, 4, 2)]
    [InlineData(1, 4, 1)]
    [Theory]
    public void CalculateTheNumberOfPagesTest1(int totalNumberOfRecords, int recordsPerPage, int shouldBeNumberOfPages)
    {
        Assert.Equal(shouldBeNumberOfPages, Pagination.CalculateTotalPages(totalNumberOfRecords, recordsPerPage));
    }
}
