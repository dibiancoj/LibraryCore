using LibraryCore.Core.ExtensionMethods;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class MemoryCacheTest
{

    #region Get Or Create Exclusive Tests

    [Fact]
    public async Task GetOrCreateExclusiveAsyncTest()
    {
        int backToDataSource = 0;

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var result = await memoryCache.GetOrCreateExclusiveAsync<IEnumerable<string>>("Test", async x =>
        {
            backToDataSource++;

            return await Task.FromResult(new string[] { "item1", "item2" });
        });

        //should have went to the data source
        Assert.Equal(1, backToDataSource);
        Assert.Equal(2, result.Count());

        //should be pulled from the cache
        var result2 = await memoryCache.GetOrCreateExclusiveAsync<IEnumerable<string>>("Test", x =>
        {
            backToDataSource++;

            throw new Exception("Shouldn't Go Go Source");
        });

        Assert.Equal(1, backToDataSource);
        Assert.Equal(2, result2.Count());
    }

    [Fact]
    public async Task GetOrCreateExclusiveAsyncShouldThrowErrorButLockIsCleared()
    {
        int backToDataSource = 0;

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await memoryCache.GetOrCreateExclusiveAsync<IEnumerable<string>>("Test", x =>
            {
                throw new Exception("Shouldn't Go Go Source");
            });
        });

        var result = await memoryCache.GetOrCreateExclusiveAsync<string>("Test", async x =>
        {
            backToDataSource++;

            return await Task.FromResult("Test123");
        });

        Assert.Equal(1, backToDataSource);
        Assert.Equal("Test123", result);
    }

    #endregion

    #region Get Or Create Exclusive With Eviction Test

    [Fact]
    public async Task GetOrCreateExclusiveWithEvictionAsyncTest()
    {
        int key1 = 1;
        int key2 = 2;
        var cancelToken = new CancellationTokenSource();

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        async Task<int> factory1(ICacheEntry x) => await Task.FromResult(key1);
        async Task<int> factory2(ICacheEntry x) => await Task.FromResult(key2);

        //pull from the cache
        Assert.Equal(1, await memoryCache.GetOrCreateExclusiveWithEvictionAsync("key1", factory1, cancelToken));
        Assert.Equal(2, await memoryCache.GetOrCreateExclusiveWithEvictionAsync("key2", factory2, cancelToken));

        key1 = 101;
        key2 = 102;

        //should get 1 since it's pulling from the cache
        Assert.Equal(1, await memoryCache.GetOrCreateExclusiveWithEvictionAsync("key1", factory1, cancelToken));
        Assert.Equal(2, await memoryCache.GetOrCreateExclusiveWithEvictionAsync("key2", factory2, cancelToken));

        //remove the cache
        cancelToken.Cancel();

        //create a new token
        cancelToken = new CancellationTokenSource();

        //both entries should be removed...so both should be the new field
        Assert.Equal(101, await memoryCache.GetOrCreateExclusiveWithEvictionAsync("key1", factory1, cancelToken));
        Assert.Equal(102, await memoryCache.GetOrCreateExclusiveWithEvictionAsync("key2", factory2, cancelToken));

        //clear it
        cancelToken.Cancel();

        //make sure it's not there now
        Assert.Equal("FromSource", await memoryCache.GetOrCreateExclusiveWithEvictionAsync(999, x => Task.FromResult("FromSource"), cancelToken));
    }

    #endregion

}
