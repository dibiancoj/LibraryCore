using LibraryCore.Core.ExtensionMethods;
using System;
using System.Net.Http;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods
{
    public class ExceptionExtensionMethodTest
    {

        internal class UnitTestException : Exception
        {
            public UnitTestException(int testVariable)
            {
                TestVariable = testVariable;
            }

            public int TestVariable { get; }
        }

        [Fact]
        public void CanParseException()
        {
            Exception exeptionRaised = new UnitTestException(9876);

            Assert.True(exeptionRaised.TryParse<UnitTestException>(out var tryCastException));
            Assert.IsType<UnitTestException>(tryCastException);
            Assert.Equal(9876, tryCastException.TestVariable);
        }

        [Fact]
        public void CantParseException()
        {
            Exception exeptionRaised = new HttpRequestException("Test Exception");

            Assert.False(exeptionRaised.TryParse<UnitTestException>(out var tryCastException));
            Assert.Null(tryCastException);
        }

    }
}
