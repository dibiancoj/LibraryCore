using LibraryCore.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryCore.Core.Delimiter
{
    public class DelimiterBuilder
    {

        #region Constructor

        /// <summary>
        /// Constructor when you dont want to add column headers
        /// </summary>
        /// <param name="delimiterBetweenColumns">Delimiter To Use Between Columns</param>
        public DelimiterBuilder(string delimiterBetweenColumns)
            : this(null, delimiterBetweenColumns)
        {
        }

        /// <summary>
        /// Constructor when you want to add column headers
        /// </summary>
        /// <param name="columnHeaders">Headers To Add To The Csv. Will Be The First Row Outputted</param>
        /// <param name="delimiterBetweenColumns">Delimiter To Use Between Columns</param>
        public DelimiterBuilder(IList<string>? columnHeaders, string delimiterBetweenColumns)
        {
            //create the string builder
            WorkingOutputWriter = new StringBuilder();

            //set the delimiter first
            ColumnDelimiter = delimiterBetweenColumns;

            //if we don't come from the other overload (where we pass in null). So make sure we have columns before trying to write them out
            if (columnHeaders.AnyWithNullCheck())
            {
                WriteOutRowOfData(new DelimiterRow(columnHeaders));
            }
        }

        #endregion

        #region Child Models

        /// <summary>
        /// Holds a row of data for the delimiter namespace. Shared between the creator and parser
        /// </summary>
        public record DelimiterRow(IList<string> ColumnData);

        #endregion

        #region Properties

        /// <summary>
        /// Holds the string builder that we write to as we are building up the return value
        /// </summary>
        private StringBuilder WorkingOutputWriter { get; }

        /// <summary>
        /// Delimiter to use between columns
        /// </summary>
        private string ColumnDelimiter { get; }

        #endregion

        #region Public Methods

        #region Add A Row Of Data

        /// <summary>
        /// Add A Row To The Output Of The CSV Data
        /// </summary>
        /// <param name="columnDataForThisRow">Row's column data</param>
        public void AddRow(IList<string> columnDataForThisRow)
        {
            //Add the row's column data
            WriteOutRowOfData(new DelimiterRow(columnDataForThisRow));
        }

        /// <summary>
        /// Adds Multiple Rows Of Data. (Bulk Add)
        /// </summary>
        /// <param name="multipleRowsOfColumnData">Multiple Rows Of Column Data</param>
        public void AddRowRange(IEnumerable<DelimiterRow> multipleRowsOfColumnData)
        {
            //for each row add it to the list
            foreach (var rowOfDataToAdd in multipleRowsOfColumnData)
            {
                //use the single method and add the item
                WriteOutRowOfData(rowOfDataToAdd);
            }
        }

        #endregion

        #region Write Data

        /// <summary>
        /// Gather the parsed CSV Output Data. Will Not Write Out The Column Header Names In The Data
        /// </summary>
        /// <returns>String To Be Outputted</returns>
        /// <remarks>Uses Environment.NewLine For Line Breaks</remarks>
        public string WriteData()
        {
            //just remove the last new line statement and return the stringbuilder in a string
            return WorkingOutputWriter.ToString(0, WorkingOutputWriter.Length - Environment.NewLine.Length);
        }

        #endregion

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Builds an individual row of data and returns the built up delimited value
        /// </summary>
        /// <param name="columnsOfDataToOutput">list of column data to output</param>
        /// <returns>Delimited String Value To Be Outputted</returns>
        private void WriteOutRowOfData(DelimiterRow columnsOfDataToOutput)
        {
            //we don't have any columns, just return an empty string
            if (!columnsOfDataToOutput.ColumnData.AnyWithNullCheck())
            {
                //have nothing, return out of the method
                return;
            }

            //dont output delimiter on this index
            int lastColumnToWriteDelimiter = columnsOfDataToOutput.ColumnData.Count - 1;

            //loop through each of the columns and add it to the string to be returned
            //using a for statement so we know which column to write the delimiter for (don't want to write it for the last item)
            for (int i = 0; i < columnsOfDataToOutput.ColumnData.Count; i++)
            {
                //grab the data to output
                var columnToWrite = columnsOfDataToOutput.ColumnData[i];

                //let's just make sure we don't have a null column
                if (columnToWrite.HasValue())
                {
                    //add the column data
                    WorkingOutputWriter.Append(columnToWrite.Replace(ColumnDelimiter, string.Empty));
                }

                //add the delimiter even if its null.
                if (i < lastColumnToWriteDelimiter)
                {
                    WorkingOutputWriter.Append(ColumnDelimiter);
                }
            }

            //add a new line now (will remove when we return the string)
            WorkingOutputWriter.Append(Environment.NewLine);
        }

        #endregion

    }
}
