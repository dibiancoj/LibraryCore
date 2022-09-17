using LibraryCore.Core.DateTimeUtilities.MockableDates;
using LibraryCore.Core.ExtensionMethods;

namespace LibraryCore.Tests.Core.DateTimeUtilities;

public class MockableDateProviders
{

    [Fact]
    public void RealImplementationTest()
    {
        IDateTimeProvider dateTimeProvider = new DateTimeProvider();

        Assert.True(dateTimeProvider.GetNow().IsBetween(DateTime.Now.AddSeconds(-30), DateTime.Now.AddSeconds(30)));
    }

    [Fact]
    public void MockedImplementationTest()
    {
        var dateToTestWith = new DateTime(2022, 5, 1);

        IDateTimeProvider dateTimeProvider = new FixedDateTimeProvider(dateToTestWith);

        Assert.Equal(dateToTestWith, dateTimeProvider.GetNow());
    }

}
