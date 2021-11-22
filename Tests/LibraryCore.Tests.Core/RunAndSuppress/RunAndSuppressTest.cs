using LibraryCore.Core.RunAndSuppress;

namespace LibraryCore.Tests.Core.RunAndSuppress;

public class RunAndSuppressTest
{

    #region Sync Tests

    [Fact(DisplayName = "Failed Sync called with default result")]
    public void FailedSyncWithDefaultResult()
    {
        string errorLogger;

        var (Successful, ResultObject) = RunAndSuppressErrors.RunAndSuppressAnyErrors<string>(() => throw new NotImplementedException(), (ex) => errorLogger = ex.ToString());

        Assert.False(Successful);
        Assert.Equal(default, ResultObject);
    }

    [Fact(DisplayName = "Failed Sync called with default result passed in")]
    public void FailedSyncWithPassedInDefaultResult()
    {
        string errorLogger;

        var (Successful, ResultObject) = RunAndSuppressErrors.RunAndSuppressAnyErrors(() => throw new NotImplementedException(), "Called Failed", (ex) => errorLogger = ex.ToString());

        Assert.False(Successful);
        Assert.Equal("Called Failed", ResultObject);
    }

    [Fact(DisplayName = "Successful Sync Call")]
    public void SuccessfulSyncCall()
    {
        string errorLogger = null;

        var (Successful, ResultObject) = RunAndSuppressErrors.RunAndSuppressAnyErrors(() => "Success Model", "Called Failed", (ex) => errorLogger = ex.ToString());

        Assert.True(Successful);
        Assert.Equal("Success Model", ResultObject);
        Assert.Null(errorLogger);
    }

    #endregion

    #region Async Tests

    private static async Task<string> SimulateTaskWithDelayAsync()
    {
        await Task.Delay(200);

        return "Success Model";
    }

    [Fact(DisplayName = "Failed Async called with default result")]
    public async Task FailedAsyncWithDefaultResult()
    {
        string errorLogger;

        var (Successful, ResultObject) = await RunAndSuppressErrors.RunAndSuppressAnyErrorsAsync<bool>(() => throw new NotImplementedException(), (ex) => errorLogger = ex.ToString());

        Assert.False(Successful);
        Assert.Equal(default, ResultObject);
    }

    [Fact(DisplayName = "Failed Async called with default result passed in")]
    public async Task FailedAsyncWithPassedInDefaultResult()
    {
        string errorLogger;

        var (Successful, ResultObject) = await RunAndSuppressErrors.RunAndSuppressAnyErrorsAsync<string>(() => throw new NotImplementedException(), () => "Called Failed", (ex) => errorLogger = ex.ToString());

        Assert.False(Successful);
        Assert.Equal("Called Failed", ResultObject);
    }

    [Fact(DisplayName = "Successful Async Call With Lambda")]
    public async Task SuccessfulAsyncCallWithLambda()
    {
        string errorLogger;

        var (Successful, ResultObject) = await RunAndSuppressErrors.RunAndSuppressAnyErrorsAsync<string>(() => SimulateTaskWithDelayAsync(), () => "Called Failed", (ex) => errorLogger = ex.ToString());

        Assert.True(Successful);
        Assert.Equal("Success Model", ResultObject);
    }

    [Fact(DisplayName = "Successful Async Call")]
    public async Task SuccessfulAsyncCall()
    {
        string errorLogger;

        var (Successful, ResultObject) = await RunAndSuppressErrors.RunAndSuppressAnyErrorsAsync<string>(SimulateTaskWithDelayAsync, () => "Called Failed", (ex) => errorLogger = ex.ToString());

        Assert.True(Successful);
        Assert.Equal("Success Model", ResultObject);
    }

    #endregion

    #region Async Value Task Tests

    private static async ValueTask<string> SimulateTaskWithDelayValueTaskAsync()
    {
        await Task.Delay(200);

        return "Success Model";
    }

    [Fact(DisplayName = "Failed Async called with default result")]
    public async Task FailedAsyncWithDefaultResultValueTask()
    {
        string errorLogger;

        var (Successful, ResultObject) = await RunAndSuppressErrors.RunAndSuppressAnyErrorsValueTaskAsync<string>(() => throw new NotImplementedException(), (ex) => errorLogger = ex.ToString());

        Assert.False(Successful);
        Assert.Equal(default, ResultObject);
    }

    [Fact(DisplayName = "Failed Async called with default result passed in")]
    public async Task FailedAsyncWithPassedInDefaultResultValueTask()
    {
        string errorLogger;

        var (Successful, ResultObject) = await RunAndSuppressErrors.RunAndSuppressAnyErrorsValueTaskAsync(() => throw new NotImplementedException(), "Called Failed", (ex) => errorLogger = ex.ToString());

        Assert.False(Successful);
        Assert.Equal("Called Failed", ResultObject);
    }

    [Fact(DisplayName = "Successful Async Call With Lambda")]
    public async Task SuccessfulAsyncCallWithLambdaValueTask()
    {
        string errorLogger;

        var (Successful, ResultObject) = await RunAndSuppressErrors.RunAndSuppressAnyErrorsValueTaskAsync(() => SimulateTaskWithDelayValueTaskAsync(), "Called Failed", (ex) => errorLogger = ex.ToString());

        Assert.True(Successful);
        Assert.Equal("Success Model", ResultObject);
    }

    [Fact(DisplayName = "Successful Async Call")]
    public async Task SuccessfulAsyncCallValueTask()
    {
        string errorLogger;

        var (Successful, ResultObject) = await RunAndSuppressErrors.RunAndSuppressAnyErrorsValueTaskAsync(SimulateTaskWithDelayValueTaskAsync, "Called Failed", (ex) => errorLogger = ex.ToString());

        Assert.True(Successful);
        Assert.Equal("Success Model", ResultObject);
    }

    #endregion

}
