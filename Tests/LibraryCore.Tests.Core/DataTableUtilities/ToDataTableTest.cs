﻿using LibraryCore.Core.DataTableUtilities;

namespace LibraryCore.Tests.Core.DataTableUtilities;

public class ToDataTableTest
{

    #region Framework

    private record DataTableTestClass(int Id, string Txt, int? NullableId, IEnumerable<DataTableTestClass> TestCollection)
    {
        public static IEnumerable<DataTableTestClass> BuildListOfTLazy(int howManyRecords)
        {
            return Enumerable.Range(0, howManyRecords)
                      .Select(x => new DataTableTestClass(x, x.ToString(), x == 0 ? null : x, new List<DataTableTestClass> { new(x, x.ToString(), null, []) }));
        }
    }

    #endregion

    #region Tests

    /// <summary>
    /// test the conversion from IEnumerable of T to a data table. This tests the single object method
    /// </summary>
    [Fact]
    public void ToDataTableTest1()
    {
        //go build 1 single item
        var singleObject = DataTableTestClass.BuildListOfTLazy(1).Single();

        //give it a name
        const string tableNameToUse = "TestTableName";

        //go grab the result of the method
        var dataTableResult = ToDataTable.BuildDataTableFromObject(singleObject, tableNameToUse);

        //check the table name
        Assert.Equal(tableNameToUse, dataTableResult.TableName);

        //check the count of how many rows we have
        Assert.Equal(1, dataTableResult.Rows.Count);

        //check the column count 
        Assert.Equal(3, dataTableResult.Columns.Count);

        //check the id field value
        Assert.Equal(0, dataTableResult.Rows[0][nameof(DataTableTestClass.Id)]);

        //check the txt field value
        Assert.Equal("0", dataTableResult.Rows[0][nameof(DataTableTestClass.Txt)]);

        //check nullable field
        Assert.Equal(DBNull.Value, dataTableResult.Rows[0][nameof(DataTableTestClass.NullableId)]);
    }

    /// <summary>
    /// test the conversion from IEnumerable of T to a data table. This tests when you pass in a list of T
    /// </summary>
    [Fact]
    public void ToDataTableTest2()
    {
        //go get the list of T to add to the data table
        var rowsToTest = DataTableTestClass.BuildListOfTLazy(3).ToArray();

        //give it a name
        const string tableNameToUse = "TestTableName";

        //go grab the result of the method
        var dataTableResult = ToDataTable.BuildDataTableFromListOfObjects(rowsToTest, tableNameToUse);

        //check the table name
        Assert.Equal(tableNameToUse, dataTableResult.TableName);

        //check how many rows we have
        Assert.Equal(rowsToTest.Length, dataTableResult.Rows.Count);

        //check the column count
        Assert.Equal(3, dataTableResult.Columns.Count);

        //check row 1
        Assert.Equal(rowsToTest[0].Id, dataTableResult.Rows[0][nameof(DataTableTestClass.Id)]);
        Assert.Equal(rowsToTest[0].Txt, dataTableResult.Rows[0][nameof(DataTableTestClass.Txt)]);
        Assert.Equal(DBNull.Value, dataTableResult.Rows[0][nameof(DataTableTestClass.NullableId)]);

        //check row 2
        Assert.Equal(rowsToTest[1].Id, dataTableResult.Rows[1][nameof(DataTableTestClass.Id)]);
        Assert.Equal(rowsToTest[1].Txt, dataTableResult.Rows[1][nameof(DataTableTestClass.Txt)]);
        Assert.Equal(rowsToTest[1].Id, dataTableResult.Rows[1][nameof(DataTableTestClass.NullableId)]);

        //check row 3
        Assert.Equal(rowsToTest[2].Id, dataTableResult.Rows[2][nameof(DataTableTestClass.Id)]);
        Assert.Equal(rowsToTest[2].Txt, dataTableResult.Rows[2][nameof(DataTableTestClass.Txt)]);
        Assert.Equal(rowsToTest[2].Id, dataTableResult.Rows[2][nameof(DataTableTestClass.NullableId)]);
    }

    /// <summary>
    /// make sure if you pass a list in to the single method name, it will raise an derror
    /// </summary>
    [Fact]
    public void ToDataTableTest3()
    {
        //run the single T method with a list, it should raise an error
        Assert.Throws<ArgumentOutOfRangeException>(() => ToDataTable.BuildDataTableFromObject(DataTableTestClass.BuildListOfTLazy(3).ToArray(), "TestTableName"));

    }

    #endregion

}
