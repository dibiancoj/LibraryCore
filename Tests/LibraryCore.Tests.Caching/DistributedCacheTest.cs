using LibraryCore.Caching;
using LibraryCore.Shared;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Tests.Caching;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<int>))]
internal partial class DistributedCacheJsonContext : JsonSerializerContext
{
}

public class DistributedCacheTest
{
    public DistributedCacheTest()
    {
        DistributedCacheServiceToTestWith = new DistributedCacheService(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
    }

    private DistributedCacheService DistributedCacheServiceToTestWith { get; }

    #region Get Or Create

    [Fact]
    public async Task GetOrCreateInCache()
    {
        var key = Guid.NewGuid().ToString();
        int callToCreateObject = 0;

        async Task<List<int>> factoryCall(DistributedCacheEntryOptions options)
        {
            callToCreateObject += 1;

            return await Task.FromResult(new List<int> { 1, 2, 3 });
        }

        async Task<List<int>> goToCacheToTestAsync()
        {
            return await DistributedCacheServiceToTestWith.GetOrCreateAsync(key, factoryCall, cancellationToken: TestContext.Current.CancellationToken);
        }

        var result = await goToCacheToTestAsync();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x == 1);
        Assert.Contains(result, x => x == 2);
        Assert.Contains(result, x => x == 3);
        Assert.Equal(1, callToCreateObject);

        //go try again to make sure it pulls from the cache
        var result2 = await goToCacheToTestAsync();

        Assert.Equal(3, result2.Count);
        Assert.Contains(result2, x => x == 1);
        Assert.Contains(result2, x => x == 2);
        Assert.Contains(result2, x => x == 3);

        //shouldn't go back to the source on the 2nd call. Should be pulled from the cache
        Assert.Equal(1, callToCreateObject);
    }

    [Fact]
    public async Task GetOrCreateIsInCache()
    {
        var key = Guid.NewGuid().ToString();

        await DistributedCacheServiceToTestWith.SetAsync(key, new List<int> { 1, 2, 3 }, TestContext.Current.CancellationToken);

        var result = await DistributedCacheServiceToTestWith.GetOrCreateAsync<List<int>>(key, (options) => throw new Exception("Should Grab From Cache"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x == 1);
        Assert.Contains(result, x => x == 2);
        Assert.Contains(result, x => x == 3);
    }

    [Trait(ErrorMessages.AotUnitTestTraitName, ErrorMessages.AotUnitTestTraitValue)]
    [Fact]
    public async Task GetOrCreateInCache_Aot()
    {
        var key = Guid.NewGuid().ToString();
        int callToCreateObject = 0;

        async Task<List<int>> factoryCall(DistributedCacheEntryOptions options)
        {
            callToCreateObject += 1;

            return await Task.FromResult(new List<int> { 1, 2, 3 });
        }

        async Task<List<int>> goToCacheToTestAsync()
        {
            return await DistributedCacheServiceToTestWith.GetOrCreateAsync(key, factoryCall, DistributedCacheJsonContext.Default.ListInt32, cancellationToken: TestContext.Current.CancellationToken);
        }

        var result = await goToCacheToTestAsync();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x == 1);
        Assert.Contains(result, x => x == 2);
        Assert.Contains(result, x => x == 3);
        Assert.Equal(1, callToCreateObject);

        //go try again to make sure it pulls from the cache
        var result2 = await goToCacheToTestAsync();

        Assert.Equal(3, result2.Count);
        Assert.Contains(result2, x => x == 1);
        Assert.Contains(result2, x => x == 2);
        Assert.Contains(result2, x => x == 3);

        //shouldn't go back to the source on the 2nd call. Should be pulled from the cache
        Assert.Equal(1, callToCreateObject);
    }

    [Fact]
    public async Task GetOrCreateInCacheWithOptions()
    {
        async Task<Guid> factoryCall(DistributedCacheEntryOptions distributedCacheEntryOptions)
        {
            distributedCacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3);

            return await Task.FromResult(Guid.NewGuid());
        }

        async Task<Guid> goToCacheToTestAsync()
        {
            return await DistributedCacheServiceToTestWith.GetOrCreateAsync(nameof(GetOrCreateInCacheWithOptions), factoryCall, cancellationToken: TestContext.Current.CancellationToken);
        }

        //should go to source
        var firstCall = await goToCacheToTestAsync();

        //should pull the same value that we pulled in the previous
        var secondCall = await goToCacheToTestAsync();

        //wait for the options to expire in 3 seconds happen
        await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        //should be a new guid
        var thirdCall = await goToCacheToTestAsync();

        Assert.Equal(firstCall, secondCall);
        Assert.NotEqual(firstCall, thirdCall);
        Assert.NotEqual(secondCall, thirdCall);
    }

    #endregion

    #region Set 

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SetWithCantFindInCache(bool addOptions)
    {
        var key = Guid.NewGuid().ToString();

        var data = new List<int> { 1, 2, 3 };

        if (addOptions)
        {
            await DistributedCacheServiceToTestWith.SetAsync(key, data, new DistributedCacheEntryOptions(), TestContext.Current.CancellationToken);
        }
        else
        {
            await DistributedCacheServiceToTestWith.SetAsync(key, data, cancellationToken: TestContext.Current.CancellationToken);
        }

        var grabItemFromCache = await DistributedCacheServiceToTestWith.GetOrCreateAsync<List<int>>(key, (options) => throw new Exception("Should Grab From Cache"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, grabItemFromCache.Count);
        Assert.Contains(grabItemFromCache, x => x == 1);
        Assert.Contains(grabItemFromCache, x => x == 2);
        Assert.Contains(grabItemFromCache, x => x == 3);
    }

    #endregion

    #region 2 Threads With One Grabbing a Lock

    [Fact]
    public async Task LockContention()
    {
        var key = Guid.NewGuid().ToString();

        //start thread 1 without awaiting it
        var startThread1 = DistributedCacheServiceToTestWith.GetOrCreateAsync(key, async (options) =>
        {
            await Task.Delay(3000, TestContext.Current.CancellationToken);

            return 9999;
        }, cancellationToken: TestContext.Current.CancellationToken);

        //kick off thread 2 which goes right away and should pick up the lock within 3 seconds
        var startThread2 = DistributedCacheServiceToTestWith.GetOrCreateAsync(key, (options) => Task.FromResult(1111), cancellationToken: TestContext.Current.CancellationToken);

        //at this point thread 1 should block thread 2. Thread 1 will return it's factory and block thread 2.
        //thread 2 will be blocked until thread 1 completes...because of the check we get after the lock. Thread 1 should win the race and return 9999
        Assert.Equal(9999, await startThread1);
        Assert.Equal(9999, await startThread2);
    }

    #endregion

    #region Cant Deserialize Bytes Exception Check

    public class CantDeserializeBytesModel
    {
        public int Id { get; set; }
    }

    [Fact]
    public async Task CantDeserializeBytes()
    {
        //set the first value
        await DistributedCacheServiceToTestWith.SetAsync("CantDeserializeBytes", "test 123", cancellationToken: TestContext.Current.CancellationToken);

        //try to deserialize it to a random class
        await Assert.ThrowsAsync<JsonException>(() => DistributedCacheServiceToTestWith.GetOrCreateAsync<CantDeserializeBytesModel>("CantDeserializeBytes", (options) => throw new NotImplementedException(), cancellationToken: TestContext.Current.CancellationToken));
    }

    #endregion

    #region Remove

    [Fact]
    public async Task RemoveCacheItem()
    {
        string key = Guid.NewGuid().ToString();

        await DistributedCacheServiceToTestWith.SetAsync(key, "Test 123", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Test 123", await DistributedCacheServiceToTestWith.GetOrCreateAsync<string>(key, (options) => throw new Exception(), cancellationToken: TestContext.Current.CancellationToken));

        await DistributedCacheServiceToTestWith.RemoveAsync(key, TestContext.Current.CancellationToken);

        //should be changed now
        Assert.Equal("ABC", await DistributedCacheServiceToTestWith.GetOrCreateAsync(key, async (options) => await Task.FromResult("ABC"), cancellationToken: TestContext.Current.CancellationToken));
    }

    #endregion

    #region Concurrency Test

    [Fact]
    public async Task CurrencyTestThreadWaitingDoesntCallSourceAgain()
    {
        string key = Guid.NewGuid().ToString();
        Guid valueToUse = Guid.NewGuid();

        //don't await this...get the 2nd thread in there
        var firstItemTakingAWhileAtDataSource = DistributedCacheServiceToTestWith.GetOrCreateAsync(key, (options) => GoToDataSourceAsync(TimeSpan.FromSeconds(5), valueToUse), cancellationToken: TestContext.Current.CancellationToken);
        var secondThreadNeedsToWaitButNotGoBackToSource = DistributedCacheServiceToTestWith.GetOrCreateAsync(key, (options) => GoToDataSourceAsync(TimeSpan.FromSeconds(5), Guid.NewGuid()), cancellationToken: TestContext.Current.CancellationToken);

        var finalResult = await Task.WhenAll(firstItemTakingAWhileAtDataSource, secondThreadNeedsToWaitButNotGoBackToSource);

        Assert.Equal(valueToUse, finalResult[0]);
        Assert.Equal(valueToUse, finalResult[1]);
    }

    private static async Task<Guid> GoToDataSourceAsync(TimeSpan delayToWait, Guid value)
    {
        await Task.Delay(Convert.ToInt32(delayToWait.TotalMilliseconds));

        return value;
    }

    #endregion

    #region Throw On Timeout

    [Fact]
    public async Task ThrowOnTimeout_GetOrCreateWithLockAsync()
    {
        //kick off a thread
        var longRunningCache = DistributedCacheServiceToTestWith.GetOrCreateAsync<string>("Test1", async (options) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            return await Task.FromResult("abc");
        }, cancellationToken: TestContext.Current.CancellationToken);

        //this should be blocked and throw
        var shouldFailBecauseOfTimeout = DistributedCacheServiceToTestWith.GetOrCreateAsync<string>("Test1", async (options) =>
        {
            return await Task.FromResult("def");
        }, acquireLockTimeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("abc", await longRunningCache);

        await Assert.ThrowsAsync<TimeoutException>(async () => await shouldFailBecauseOfTimeout);
    }

    #endregion

}
