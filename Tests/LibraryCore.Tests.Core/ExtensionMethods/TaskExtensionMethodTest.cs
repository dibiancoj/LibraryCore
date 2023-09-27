namespace LibraryCore.Tests.Core.ExtensionMethods;

public class TaskExtensionMethodTest
{
    private static async Task<string> AsyncStub1Method()
    {
        await Task.Delay(50);

        return "Test 123";
    }

    private static async Task<string> AsyncStub2Method()
    {
        await Task.Delay(50);

        return "T1";
    }

    private static async Task<int> AsyncStub3Method()
    {
        await Task.Delay(50);

        return 1000;
    }

    [Fact]
    public async Task ThenResultTest()
    {
        Assert.Equal("T", await AsyncStub1Method().Then(tsk => tsk[..1]));
    }

    [Fact]
    public async Task ThenResultAwaitContinuationTest()
    {
        Assert.Equal("T1", await AsyncStub1Method().Then(tsk => AsyncStub2Method()));
    }

    [Fact]
    public async Task ChainedAwaits()
    {
        Assert.Equal(1000, await AsyncStub1Method().Then(tsk => AsyncStub2Method()).Then(tsk => AsyncStub3Method()));
    }
}
