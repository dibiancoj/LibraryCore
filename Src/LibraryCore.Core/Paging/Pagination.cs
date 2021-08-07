using System;

namespace LibraryCore.Core.Paging
{
    /// <summary>
    /// Helps page data for a data set
    /// </summary>
    public static class Pagination
    {
        //** Paging of the data is in IOrderedEnumerable Extension Methods **

        /// <summary>
        /// Calculates Total Number Of Pages In This Grid (Property Of Total Above - Need Additional Data To Calculate)
        /// </summary>
        /// <param name="howManyTotalRecordsInDataSet">Total Number Of Records (Not Just This Page But In The Entire RecordSet)</param>
        /// <param name="howManyRecordsPerPage">How Many Records Per Page</param>
        /// <returns>Number Of Pages</returns>
        public static int CalculateTotalPages(int howManyTotalRecordsInDataSet, int howManyRecordsPerPage)
        {
            //calculate how many pages we have
            double conversion = (double)howManyTotalRecordsInDataSet / howManyRecordsPerPage;

            //do we have an even amount
            return ((conversion % 1) == 0) ?
              
                //we have an even amount
              Convert.ToInt32(conversion) :

              //we have an uneven amount...so grab the floor then add 1
              Convert.ToInt32(Math.Floor(conversion)) + 1;
        }

    }
}
