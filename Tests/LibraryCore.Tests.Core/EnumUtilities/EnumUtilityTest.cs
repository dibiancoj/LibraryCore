using LibraryCore.Core.EnumUtilities;
using System;
using System.Linq;
using Xunit;

namespace LibraryCore.Tests.Core.EnumUtilities
{
    public class EnumUtilityTest
    {

        #region Framework

        private class DescriptionTextAttribute : Attribute
        {
            public DescriptionTextAttribute(string descriptionText)
            {
                Description = descriptionText;
            }

            public string Description { get; }
        }

        public enum TestEnum : int
        {
            [DescriptionText("City_123")]
            City = 0,
            State = 1,
            [DescriptionText("Country_123")]
            Country = 2,
            Planet = 4
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

        #endregion

    }
}
