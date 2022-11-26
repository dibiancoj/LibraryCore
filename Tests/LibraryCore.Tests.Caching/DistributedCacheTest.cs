using LibraryCore.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LibraryCore.Tests.Caching;

public class DistributedCacheTest
{

    public DistributedCacheTest()
    {
        DistributedCacheServiceToTestWith = new DistributedCacheService(new FullMockIDistributedCache());
    }

    private DistributedCacheService DistributedCacheServiceToTestWith { get; }

    #region Get Or Create

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task GetOrCreateInCache(bool addOptions)
    {
        var key = Guid.NewGuid().ToString();
        int callToCreateObject = 0;

        async Task<List<int>> factoryCall()
        {
            callToCreateObject += 1;

            return await Task.FromResult(new List<int> { 1, 2, 3 });
        }

        async Task<List<int>> goToCacheToTestAsync()
        {
            if (addOptions)
            {
                return await DistributedCacheServiceToTestWith.GetOrCreateAsync(key, factoryCall, new DistributedCacheEntryOptions());
            }

            return await DistributedCacheServiceToTestWith.GetOrCreateAsync(key, factoryCall);
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

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task GetOrCreateIsInCache(bool addOptions)
    {
        var key = Guid.NewGuid().ToString();

        await DistributedCacheServiceToTestWith.SetAsync(key, new List<int> { 1, 2, 3 });

        var result = addOptions ?
                            await DistributedCacheServiceToTestWith.GetOrCreateAsync<List<int>>(key, () => throw new Exception("Should Grab From Cache"), new DistributedCacheEntryOptions()) :
                            await DistributedCacheServiceToTestWith.GetOrCreateAsync<List<int>>(key, () => throw new Exception("Should Grab From Cache"));

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x == 1);
        Assert.Contains(result, x => x == 2);
        Assert.Contains(result, x => x == 3);
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
            await DistributedCacheServiceToTestWith.SetAsync(key, data, new DistributedCacheEntryOptions());
        }
        else
        {
            await DistributedCacheServiceToTestWith.SetAsync(key, data);
        }

        var grabItemFromCache = await DistributedCacheServiceToTestWith.GetOrCreateAsync<List<int>>(key, () => throw new Exception("Should Grab From Cache"));

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
        var startThread1 = DistributedCacheServiceToTestWith.GetOrCreateAsync(key, async () =>
        {
            await Task.Delay(3000);

            return 9999;
        });

        //kick off thread 2 which goes right away and should pick up the lock within 3 seconds
        var startThread2 = DistributedCacheServiceToTestWith.GetOrCreateAsync(key, () => Task.FromResult(1111));

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
        await DistributedCacheServiceToTestWith.SetAsync("CantDeserializeBytes", "test 123");

        //try to deserialize it to a random class
        await Assert.ThrowsAsync<JsonException>(() => DistributedCacheServiceToTestWith.GetOrCreateAsync<CantDeserializeBytesModel>("CantDeserializeBytes", () => throw new NotImplementedException()));
    }

    #endregion

    #region Remove

    [Fact]
    public async Task RemoveCacheItem()
    {
        string key = Guid.NewGuid().ToString();

        await DistributedCacheServiceToTestWith.SetAsync(key, "Test 123");

        Assert.Equal("Test 123", await DistributedCacheServiceToTestWith.GetOrCreateAsync<string>(key, () => throw new Exception()));

        await DistributedCacheServiceToTestWith.RemoveAsync(key);

        //should be changed now
        Assert.Equal("ABC", await DistributedCacheServiceToTestWith.GetOrCreateAsync(key, async () => await Task.FromResult("ABC")));
    }

    #endregion

}
