using LibraryCore.Core.ExtensionMethods;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class DateTimeExtensionMethodTest
{

    [Fact]
    public void DateTimeIsNotBetweenBeforeStartDate()
    {
        Assert.False(new DateTime(2019, 5, 1).IsBetween(new DateTime(2019, 6, 1), new DateTime(2019, 7, 1)));
    }

    [Fact]
    public void DateTimeIsNotBetweenAfterEndDate()
    {
        Assert.False(new DateTime(2019, 8, 1).IsBetween(new DateTime(2019, 6, 1), new DateTime(2019, 7, 1)));
    }

    [Fact]
    public void DateTimeIsBetween()
    {
        Assert.True(new DateTime(2019, 6, 25).IsBetween(new DateTime(2019, 6, 1), new DateTime(2019, 7, 1)));
    }

}

