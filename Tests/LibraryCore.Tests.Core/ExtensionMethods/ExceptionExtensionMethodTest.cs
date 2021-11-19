using LibraryCore.Core.ExtensionMethods;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods;

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

    [Fact]
    public void SingleExceptionTree()
    {
        var oneException = new HttpRequestException("Test Exception");

        var result = oneException.ExceptionTree().ToArray();

        Assert.Single(result);
        Assert.Equal(result.Single(), oneException);
    }

    [Fact]
    public void MultipleExceptionsInTree()
    {
        var innerInnerExeption = new ArgumentNullException("Inner Inner Message Exception");
        var innerExeption = new NullReferenceException("Inner Message Exception", innerInnerExeption);
        var rootException = new HttpRequestException("Test Exception", innerExeption);

        var result = rootException.ExceptionTree().ToList();

        Assert.Equal(3, result.Count);

        //root error
        Assert.Equal(result.OfType<HttpRequestException>().Single(), rootException);
        Assert.IsType<HttpRequestException>(result.ElementAt(0));

        //inner exception of the root
        Assert.Equal(result.OfType<NullReferenceException>().Single(), innerExeption);
        Assert.IsType<NullReferenceException>(result.ElementAt(1));

        //inner off of the inner
        Assert.Equal(result.OfType<ArgumentNullException>().Single(), innerInnerExeption);
        Assert.IsType<ArgumentNullException>(result.ElementAt(2));
    }

}
