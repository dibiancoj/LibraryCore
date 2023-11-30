using LibraryCore.Core.EnumUtilities;
using System.ComponentModel;

namespace LibraryCore.Tests.Core.EnumUtilities;

public class EnumUtilityTest
{

    #region Framework

    [AttributeUsage(AttributeTargets.Field)]
    private class DescriptionTextAttribute(string descriptionText) : Attribute
    {
        public string Description { get; } = descriptionText;
    }

    [Flags]
    public enum TestEnum : int
    {
        [DescriptionText("City_123")]
        City = 0,
        State = 1,
        [DescriptionText("Country_123")]
        Country = 2,
        Planet = 4
    }

    [Flags]
    public enum LookupTestEnum : int
    {
        [Description("My City")]
        City = 0,
        [Description("My State")]
        State = 1,
    }

    #endregion

    #region Unit Tests

    #region Try Parse To Nullable

    [Fact]
    public void CanParseToNullable()
    {
        Assert.Equal(TestEnum.Planet, EnumUtility.TryParseToNullable<TestEnum>("Planet"));
    }

    [Fact]
    public void CantBeParsedToNullable()
    {
        Assert.Null(EnumUtility.TryParseToNullable<TestEnum>("NotParseable"));
    }

    #endregion

    #region Get Values

    [Fact]
    public void EnumGetValues()
    {
        //grab the values
        var EnumValuesToTest = EnumUtility.GetValuesLazy<TestEnum>().ToArray();

        //make sure we have the number of enum values
        Assert.Equal(4, EnumValuesToTest.Length);

        //make sure we have the 3 enum values now
        Assert.Contains(EnumValuesToTest, x => x == TestEnum.City);
        Assert.Contains(EnumValuesToTest, x => x == TestEnum.State);
        Assert.Contains(EnumValuesToTest, x => x == TestEnum.Country);
        Assert.Contains(EnumValuesToTest, x => x == TestEnum.Planet);
    }

    #endregion

    #region Custom Attribute

    [Fact]
    public void EnumLookupTest()
    {
        var result = EnumUtility.EnumLookupTable<LookupTestEnum>();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Key == LookupTestEnum.City && x.Value == "My City");
        Assert.Contains(result, x => x.Key == LookupTestEnum.State && x.Value == "My State");
    }

    /// <summary>
    /// Test getting a custom attribute off of an enum
    /// </summary>
    [Fact]
    public void GetCustomAttributeFromEnum()
    {
        //check the custom attribute values
        Assert.Equal("City_123", EnumUtility.CustomAttributeGet<DescriptionTextAttribute>(TestEnum.City).Description);
        Assert.Equal("Country_123", EnumUtility.CustomAttributeGet<DescriptionTextAttribute>(TestEnum.Country).Description);

        //make sure it throws if we cant find a value
        Assert.Throws<NullReferenceException>(() => EnumUtility.CustomAttributeGet<DescriptionTextAttribute>(TestEnum.State));
    }

    /// <summary>
    /// Test getting a custom attribute off of an enum
    /// </summary>
    [Fact]
    public void TryGetCustomAttributeFromEnum()
    {
        //make sure this value is null
        Assert.Null(EnumUtility.CustomAttributeTryGet<DescriptionTextAttribute>(TestEnum.State));
    }

    [InlineData(TestEnum.City, true)]
    [InlineData(TestEnum.State, false)]
    [Theory]
    public void IsAttributeDefined(TestEnum testEnumValue, bool expectedIsDefined)
    {
        Assert.Equal(expectedIsDefined, EnumUtility.AttributeIsDefined<DescriptionTextAttribute>(testEnumValue));
    }

    #endregion

    #region Bit Mask

    #region Enum Utility

    [Fact]
    public void BitMaskAddAValue()
    {
        var startingValue = TestEnum.Planet;

        var result = EnumUtility.BitMaskSelectedItems(EnumUtility.BitMaskAddItem(startingValue, TestEnum.State)).ToArray();

        //city = 0...so always included
        Assert.Contains(result, x => x == TestEnum.City);
        Assert.DoesNotContain(result, x => x == TestEnum.Country);
        Assert.Contains(result, x => x == TestEnum.Planet);
        Assert.Contains(result, x => x == TestEnum.State);
    }

    [Fact]
    public void BitMaskRemoveItem()
    {
        var workingValue = EnumUtility.BitMaskAddItem(EnumUtility.BitMaskAddItem(TestEnum.Planet, TestEnum.State), TestEnum.Country);

        workingValue = EnumUtility.BitMaskRemoveItem(workingValue, TestEnum.State);

        var selectedItems = EnumUtility.BitMaskSelectedItems(workingValue).ToArray();

        Assert.DoesNotContain(selectedItems, x => x == TestEnum.State);
        Assert.Contains(selectedItems, x => x == TestEnum.Planet);
        Assert.Contains(selectedItems, x => x == TestEnum.Country);
    }

    [Fact]
    public void BitMaskContainsAValue()
    {
        var workingValue = TestEnum.Planet;

        Assert.True(EnumUtility.BitMaskContainsValue(workingValue, TestEnum.Planet));

        //value = 0 so always included
        Assert.True(EnumUtility.BitMaskContainsValue(workingValue, TestEnum.City));
        Assert.False(EnumUtility.BitMaskContainsValue(workingValue, TestEnum.State));

        Assert.False(EnumUtility.BitMaskContainsValue(workingValue, TestEnum.Country));

        workingValue = EnumUtility.BitMaskAddItem(workingValue, TestEnum.Country);

        Assert.True(EnumUtility.BitMaskContainsValue(workingValue, TestEnum.Planet));
        Assert.True(EnumUtility.BitMaskContainsValue(workingValue, TestEnum.Country));
        Assert.False(EnumUtility.BitMaskContainsValue(workingValue, TestEnum.State));

        //value = 0 so always included
        Assert.True(EnumUtility.BitMaskContainsValue(workingValue, TestEnum.City));
    }

    [Fact]
    public void BitMaskSelectedValues()
    {
        var workingValue = EnumUtility.BitMaskSelectedItems(EnumUtility.BitMaskAddItem(TestEnum.Planet, TestEnum.State));

        //always contains because value = 0
        Assert.Contains(workingValue, x => x == TestEnum.City);
        Assert.Contains(workingValue, x => x == TestEnum.State);
        Assert.Contains(workingValue, x => x == TestEnum.Planet);
        Assert.DoesNotContain(workingValue, x => x == TestEnum.Country);
    }

    #endregion

    #region Bit Mask Builder

    [Fact]
    public void BitMaskAddAValueBuilder()
    {
        var result = new BitMaskBuilder<TestEnum>(TestEnum.Planet)
                                        .AddItem(TestEnum.State)
                                        .SelectedItems();

        //city = 0...so always included
        Assert.Contains(result, x => x == TestEnum.City);
        Assert.DoesNotContain(result, x => x == TestEnum.Country);
        Assert.Contains(result, x => x == TestEnum.Planet);
        Assert.Contains(result, x => x == TestEnum.State);
    }

    [Fact]
    public void BitMaskRemoveItemBuilder()
    {
        var selectedItems = new BitMaskBuilder<TestEnum>(TestEnum.Planet)
                                                .AddItem(TestEnum.State)
                                                .AddItem(TestEnum.Country)
                                                .RemoveItem(TestEnum.State)
                                                .SelectedItems();

        Assert.DoesNotContain(selectedItems, x => x == TestEnum.State);
        Assert.Contains(selectedItems, x => x == TestEnum.Planet);
        Assert.Contains(selectedItems, x => x == TestEnum.Country);
    }

    [Fact]
    public void BitMaskContainsAValueBuilder()
    {
        var builder = new BitMaskBuilder<TestEnum>(TestEnum.Planet);

        Assert.True(builder.ContainsValue(TestEnum.Planet));

        //value = 0 so always included
        Assert.True(builder.ContainsValue(TestEnum.City));
        Assert.False(builder.ContainsValue(TestEnum.State));
        Assert.False(builder.ContainsValue(TestEnum.Country));

        builder.AddItem(TestEnum.Country);

        Assert.True(builder.ContainsValue(TestEnum.City));
        Assert.False(builder.ContainsValue(TestEnum.State));
        Assert.True(builder.ContainsValue(TestEnum.Planet));
        Assert.True(builder.ContainsValue(TestEnum.Country));
    }

    [Fact]
    public void BitMaskSelectedValuesBuilder()
    {
        var result = new BitMaskBuilder<TestEnum>(TestEnum.Planet)
                                    .AddItem(TestEnum.State)
                                    .SelectedItems();

        //always contains because value = 0
        Assert.Contains(result, x => x == TestEnum.City);
        Assert.Contains(result, x => x == TestEnum.State);
        Assert.Contains(result, x => x == TestEnum.Planet);
        Assert.DoesNotContain(result, x => x == TestEnum.Country);
    }

    #endregion

    #endregion

    #endregion

}
