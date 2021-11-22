using LibraryCore.Core.ReferenceData;

namespace LibraryCore.Tests.Core.ReferenceData;

public class UnitedStatesTest
{
    [Fact]
    public void UnitedStateListingTest()
    {
        var results = UnitedStates.StateListing();

        Assert.Equal(52, results.Count());
        Assert.Single(results, x => x.Id == "NY" && x.Description == "New York");
        Assert.Single(results, x => x.Id == "NJ" && x.Description == "New Jersey");
    }
}
