using LibraryCore.Core.DataTypes;
using System;
using System.Collections.Generic;
using Xunit;

namespace LibraryCore.Tests.Core.DataTypes
{
    public class PrimitiveTypesTest
    {
        /// <summary>
        /// Test Primitive types. All these items should be found in the list
        /// </summary>
        [InlineData(typeof(string))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(bool?))]
        [InlineData(typeof(Int16))]
        [InlineData(typeof(Int16?))]
        [InlineData(typeof(int))]
        [InlineData(typeof(int?))]
        [InlineData(typeof(Int64))]
        [InlineData(typeof(Int64?))]
        [InlineData(typeof(double))]
        [InlineData(typeof(double?))]
        [InlineData(typeof(float))]
        [InlineData(typeof(float?))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(decimal?))]
        [Theory]
        public void PrimitiveTypesTest1(Type TypeToTest)
        {
            //make sure this is in the list
            Assert.True(PrimitiveTypes.PrimitiveTypesSelect().Contains(TypeToTest));
        }

        /// <summary>
        /// Test Primitive types. All these items should NOT be found in the list
        /// </summary>
        [InlineData(typeof(IEnumerable<double>))]
        [InlineData(typeof(object))]
        [InlineData(typeof(List<double>))]
        [Theory]
        public void PrimitiveTypesTest2(Type TypeToTest)
        {
            //make sure this item is NOT in the list
            Assert.False(PrimitiveTypes.PrimitiveTypesSelect().Contains(TypeToTest));
        }
    }
}
