using LibraryCore.Core.DataTypes.Unions;
using System;
using Xunit;

namespace LibraryCore.Tests.Core.DataTypes
{
    public class UnionTest
    {
        [Fact]
        public void UnionTypeWithObject()
        {
            var unionToTest = new Union<string, DateTime>("test");

            Assert.True(unionToTest.Is<string>());
            Assert.False(unionToTest.Is<DateTime>());
            Assert.False(unionToTest.Is<bool>());

            Assert.Equal("test", unionToTest.As<string>());
            Assert.Equal(default, unionToTest.As<DateTime>());
            Assert.Equal(default, unionToTest.As<bool>());
        }

        [Fact]
        public void UnionTypeWithStruct()
        {
            var dateToUse = DateTime.Now;

            var unionToTest = new Union<string, DateTime>(dateToUse);

            Assert.False(unionToTest.Is<string>());
            Assert.True(unionToTest.Is<DateTime>());
            Assert.False(unionToTest.Is<bool>());

            Assert.Equal(default, unionToTest.As<string>());
            Assert.Equal(dateToUse, unionToTest.As<DateTime>());
            Assert.Equal(default, unionToTest.As<bool>());
        }

        [Fact]
        public void UnionTypeWithNullableObjectThatIsNull()
        {
            var unionToTest = new Union<bool?, DateTime>(null);

            Assert.True(unionToTest.Is<bool?>());
            Assert.False(unionToTest.Is<DateTime>());

            Assert.Equal(default, unionToTest.As<string>());
            Assert.False(unionToTest.As<bool?>().HasValue);
        }

        [Fact]
        public void UnionTypeWithNullableObjectThatHasAValue()
        {
            var unionToTest = new Union<bool, DateTime>(true);

            Assert.True(unionToTest.Is<bool>());
            Assert.False(unionToTest.Is<DateTime>());

            Assert.Equal(default, unionToTest.As<string>());
            Assert.True(unionToTest.As<bool>());
        }

        #region Realistic Example

        public record ValidPasswordResponse(string UserName);
        public record InvalidPasswordResponse(string ErrorMessage);

        [Fact]
        public void WithValidPasswordResponse()
        {
            var unionToTest = new Union<ValidPasswordResponse, InvalidPasswordResponse>(new ValidPasswordResponse("User1"));

            if (unionToTest.Is<ValidPasswordResponse>())
            {
                Assert.Equal("User1", unionToTest.As<ValidPasswordResponse>().UserName);
            }
            else
            {
                throw new Exception("Unit test should not hit this exception");
            }
        }

        [Fact]
        public void WithInValidPasswordResponse()
        {
            var unionToTest = new Union<ValidPasswordResponse, InvalidPasswordResponse>(new InvalidPasswordResponse("Invalid user name and or password"));

            if (unionToTest.Is<ValidPasswordResponse>())
            {
                throw new Exception("Unit test should not hit this exception");
            }
            else
            {
                Assert.Equal("Invalid user name and or password", unionToTest.As<InvalidPasswordResponse>().ErrorMessage);
            }
        }

        #endregion

        #region 3 Type Test

        [Fact]
        public void ThreeDataTypeTest()
        {
            var dateTimeToTestWith = DateTime.Now;
            var unionToTest = new Union<string, DateTime, bool>("test");

            Assert.True(unionToTest.Is<string>());
            Assert.False(unionToTest.Is<DateTime>());
            Assert.False(unionToTest.Is<bool>());

            Assert.Equal("test", unionToTest.As<string>());
            Assert.Equal(default, unionToTest.As<DateTime>());
            Assert.Equal(default, unionToTest.As<bool>());

            //test the date now
            unionToTest = new Union<string, DateTime, bool>(dateTimeToTestWith);

            //make sure the date is good
            Assert.Equal(dateTimeToTestWith, unionToTest.As<DateTime>());

            //test the boolean now
            unionToTest = new Union<string, DateTime, bool>(true);

            //make sure its true
            Assert.True(unionToTest.As<bool>());
        }

        #endregion
    }
}
