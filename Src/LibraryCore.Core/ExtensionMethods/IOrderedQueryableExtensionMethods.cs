using System.Diagnostics.CodeAnalysis;
using LibraryCore.Shared;

namespace LibraryCore.Core.ExtensionMethods;

public static class IOrderedQueryableExtensionMethods
{
    /// <summary>
    /// Takes The IOrderedQueryable And Grabs The Current Page We Are On. We use iordered queryable because the data should always be ordered when paged. EF actually pages you order it otherwise it will through an error.
    /// Making the developer pass in an ordered set so we don't get any silent issues because the data isn't ordered
    /// </summary>
    /// <typeparam name="T">Type Of Records That Are Returned</typeparam>
    /// <param name="queryToModify">Query To Modify</param>
    /// <param name="currentPageNumber">What Page Number Are You Currently On</param>
    /// <param name="howManyRecordsPerPage">How Many Records Per Page</param>
    /// <returns>IQueryable Of T</returns>
    //[LinqToObjectsCompatible]
    //[EntityFrameworkCompatible]

    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static IQueryable<T> PaginateResults<T>(this IOrderedQueryable<T> queryToModify, int currentPageNumber, int howManyRecordsPerPage)
    {
        //if you have a list of <T>...you can use the order by overload and pass in an expression<func... that will give you back an IOrderedQueryable
        //ie: GridDataSource.AsQueryable().OrderBy(SortPropertySelector).PaginateResults(PageIndex, HowManyPerPage)

        //how to call this
        //sql = sql.OrderBy(sidx, isAscending).PaginateResults(page, rows);

        //run a quick check to make sure the page number is ok
        if (currentPageNumber == 0)
        {
            throw new IndexOutOfRangeException("Current Page Number Can't Be 0. Use 1 For The First Page");
        }

        //go skip however many pages we are past...and take only x amount of records per page
        return queryToModify.Skip((currentPageNumber - 1) * howManyRecordsPerPage).Take(howManyRecordsPerPage).AsQueryable();
    }
}
