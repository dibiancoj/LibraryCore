using LibraryCore.Core.DataTypes;

namespace LibraryCore.Tests.Core.DataTypes;

public class PrimitiveTypesTest
{
    /// <summary>
    /// Test Primitive types. All these items should be found in the list
    /// </summary>
    [InlineData(typeof(string))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(bool?))]
    [InlineData(typeof(DateOnly))]
    [InlineData(typeof(DateOnly?))]
    [InlineData(typeof(TimeOnly))]
    [InlineData(typeof(TimeOnly?))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTime?))]
    [InlineData(typeof(short))]
    [InlineData(typeof(short?))]
    [InlineData(typeof(int))]
    [InlineData(typeof(int?))]
    [InlineData(typeof(long))]
    [InlineData(typeof(long?))]
    [InlineData(typeof(double))]
    [InlineData(typeof(double?))]
    [InlineData(typeof(float))]
    [InlineData(typeof(float?))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(decimal?))]
    [Theory]
    public void PrimitiveTypesTest1(Type TypeToTest) => Assert.True(PrimitiveTypes.PrimitiveTypesSelect().Contains(TypeToTest));

    /// <summary>
    /// Test Primitive types. All these items should NOT be found in the list
    /// </summary>
    [InlineData(typeof(IEnumerable<double>))]
    [InlineData(typeof(object))]
    [InlineData(typeof(List<double>))]
    [Theory]
    public void PrimitiveTypesTest2(Type TypeToTest) => Assert.False(PrimitiveTypes.PrimitiveTypesSelect().Contains(TypeToTest));

}
