using LibraryCore.Core.ExtensionMethods;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public static class GuidExtensionMethodTest
{
    [Fact]
    public static async Task SequentialGuidCreatorTest()
    {
        var baseGuid = Guid.Empty;

        await Task.Delay(TimeSpan.FromSeconds(1));

        var secondGuid = baseGuid.SequentialGuidCreate();

        await Task.Delay(TimeSpan.FromSeconds(1));

        var thirdGuid = secondGuid.SequentialGuidCreate();

        await Task.Delay(TimeSpan.FromSeconds(1));

        var fourthGuid = thirdGuid.SequentialGuidCreate();

        var allGuids = new[] { baseGuid, secondGuid, thirdGuid, fourthGuid }
                        .OrderBy(x => x)
                        .ToArray();

        Assert.True(allGuids[0] == baseGuid);
        Assert.True(allGuids[1] == secondGuid);
        Assert.True(allGuids[2] == thirdGuid);
        Assert.True(allGuids[3] == fourthGuid);
    }
}
