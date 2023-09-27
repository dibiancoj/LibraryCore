namespace LibraryCore.Tests.Core.ExtensionMethods;

public class TaskExtensionMethodTest
{
    private static async Task<T> AsyncStub<T>(T result)
    {
        await Task.Delay(50);

        return result;
    }

    [Fact]
    public async Task ThenResultTest()
    {
        Assert.Equal("T", await AsyncStub("Test 123").Then(tsk => tsk[..1]));
    }

    [Fact]
    public async Task ThenResultAwaitContinuationTest()
    {
        Assert.Equal("T1", await AsyncStub("Test 123").Then(tsk => AsyncStub("T1")));
    }

    [Fact]
    public async Task ChainedAwaits()
    {
        Assert.Equal(1000, await AsyncStub("Test 123").Then(tsk => AsyncStub("T1")).Then(tsk => AsyncStub(1000)));
    }
}
