using LibraryCore.Core.DiagnosticUtilities;

namespace LibraryCore.Tests.Core.DiagnosticUtilities;

public class DiagnosticUtilityTest
{
    [Fact]
    public async Task SpinWaitUntilWithPredicateMatches()
    {
        int timesCalled = 0;

        var result = await DiagnosticUtility.SpinUntilAsync(async () =>
        {
            //force it to delay
            await Task.Delay(10);

            timesCalled++;

            return timesCalled == 3;
        }, TimeSpan.FromMilliseconds(3), TimeSpan.FromSeconds(10));

        Assert.True(result);
    }

    [Fact]
    public async Task SpinWaitUntilWithNoPredicateFound()
    {
        int timesCalled = 0;

        var result = await DiagnosticUtility.SpinUntilAsync(async () =>
        {
            //force it to delay
            await Task.Delay(10);

            return timesCalled == int.MaxValue;
        }, TimeSpan.FromMilliseconds(3), TimeSpan.FromSeconds(3));

        Assert.False(result);
    }
}
