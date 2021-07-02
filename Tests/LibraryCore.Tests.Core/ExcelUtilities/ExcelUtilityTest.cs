using LibraryCore.Core.ExcelUtilities;
using Xunit;

namespace LibraryCore.Tests.Core.ExcelUtilities
{
    public class ExcelUtilityTest
    {
        #region Column Name To Int

        /// <summary>
        /// Test column name to number (for excel)
        /// </summary>
        [Fact]
        public void ExcelToolsColumnNameToIntTest1()
        {
            Assert.Equal(1, ExcelUtility.ColumnLetterToColumnIndex("A"));
            Assert.Equal(27, ExcelUtility.ColumnLetterToColumnIndex("AA"));
            Assert.Equal(4, ExcelUtility.ColumnLetterToColumnIndex("D"));

        }

        #endregion

        #region Column Number To Letter

        /// <summary>
        /// Test column number to letter (for excel)
        /// </summary>
        [Fact]
        public void ExcelToolsColumnNumberToLetterTest1()
        {
            Assert.Equal("AA", ExcelUtility.ColumnIndexToColumnLetter(27));
            Assert.Equal("A", ExcelUtility.ColumnIndexToColumnLetter(1));
            Assert.Equal("D", ExcelUtility.ColumnIndexToColumnLetter(4));
        }

        #endregion
    }
}
