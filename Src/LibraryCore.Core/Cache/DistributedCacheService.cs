using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace LibraryCore.Core.Cache;

public class DistributedCacheService
{

    #region Properties

    /// <summary>
    /// Holds a lookup for all the cache locks. You can't lock an async method so we use Semaphore. 
    /// </summary>
    private static ConcurrentDictionary<object, SemaphoreSlim> CacheLocksLookup { get; } = new ConcurrentDictionary<object, SemaphoreSlim>();

    private IDistributedCache DistributedCache { get; }

    #endregion

    #region Constructor

    public DistributedCacheService(IDistributedCache distributedCache)
    {
        DistributedCache = distributedCache;
    }

    #endregion

    #region Public Methods

    public async Task<TItem> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory)
    {
        return await GetOrCreateAsync(key, factory, new DistributedCacheEntryOptions());
    }

    public async Task<TItem> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory, DistributedCacheEntryOptions distributedCacheEntryOptions)
    {
        var tryToGrabFromCacheResult = await TryToGetValueFromCache<TItem>(key);

        //we have it just return it
        if (tryToGrabFromCacheResult.TryGetItemFromResult(out var tryToGetItemFromCacheResult))
        {
            return tryToGetItemFromCacheResult;
        }

        //so we couldn't find it on our first shot. we need to take a lock now
        //couldn't grab it...we need to set the lock
        var asyncLockToUse = AcquireLock(key);

        try
        {
            //grab the count in a variable. If = 1...then you are the thread that is causing the blocking
            int i = asyncLockToUse.CurrentCount;

            //take the lock and wait for it
            await asyncLockToUse.WaitAsync();

            //if we are the thread that is blocking then don't try to fetch it again. If we were waiting because of a another thread...
            //then the value should be in the cache and go fetch it (still verify its in the cache and don't assume because of contention)
            if (i == 0)
            {
                //since we took a lock the value might be there from another thread. so try to get 1 1 more time. 
                var tryToGrabFromCacheAfterLockResult = await TryToGetValueFromCache<TItem>(key);

                //check if we have it
                if (tryToGrabFromCacheAfterLockResult.TryGetItemFromResult(out var tryToGetItemFromCacheResultAfterLock))
                {
                    return tryToGetItemFromCacheResultAfterLock;
                }
            }

            //didn't find it...go create it, set it in the cache, and return it
            return await SetAsync(key, await factory(), distributedCacheEntryOptions);
        }
        finally
        {
            //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
            //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
            asyncLockToUse.Release();
        }
    }

    public async Task<TItem> SetAsync<TItem>(string key, TItem itemToAdd)
    {
        return await SetAsync(key, itemToAdd, new DistributedCacheEntryOptions());
    }

    public async Task<TItem> SetAsync<TItem>(string key, TItem itemToAdd, DistributedCacheEntryOptions distributedCacheEntryOptions)
    {
        //can't pass null for the distributed options
        await DistributedCache.SetAsync(key, SerializeToBytes(itemToAdd), distributedCacheEntryOptions);

        return itemToAdd;
    }

    public Task RemoveAsync(string key) => DistributedCache.RemoveAsync(key);

    #endregion

    #region Private Helper Methods

    private async Task<TryToGetInCacheResult<TItem>> TryToGetValueFromCache<TItem>(string key)
    {
        var tryToFindInDistributedCache = await DistributedCache.GetAsync(key);

        return tryToFindInDistributedCache == null ?
                            TryToGetInCacheResult<TItem>.IsNotFoundInCache() :
                            TryToGetInCacheResult<TItem>.IsFoundInCache(DeserializeFromBytes<TItem>(tryToFindInDistributedCache));
    }

    /// <summary>
    /// In a method incase we need to change the default serialization
    /// </summary>
    private static byte[] SerializeToBytes<T>(T model) => JsonSerializer.SerializeToUtf8Bytes(model);

    /// <summary>
    /// In a method incase we need to change the default serialization
    /// </summary>
    private static T DeserializeFromBytes<T>(byte[] bytes) => JsonSerializer.Deserialize<T>(bytes)!; //! = there will be most likely a json exception instead of returning null. Visible in unit tests

    private static SemaphoreSlim AcquireLock(object key) => CacheLocksLookup.GetOrAdd(key, (x) => new SemaphoreSlim(1, 1));

    #endregion

    #region Models

    /// <summary>
    /// Splitting this out into it's own class for nullability checks a real value if T is populated since we can't use out parameters in async methods
    /// </summary>
    private class TryToGetInCacheResult<T>
    {
        private bool FoundInCache { get; init; }
        private T? ItemFoundInCache { get; init; }

        internal static TryToGetInCacheResult<T> IsNotFoundInCache() => new() { FoundInCache = false };
        internal static TryToGetInCacheResult<T> IsFoundInCache(T itemFoundInCache) => new() { FoundInCache = true, ItemFoundInCache = itemFoundInCache };

        internal bool TryGetItemFromResult([NotNullWhen(true)] out T? tryToRetrieveItemFoundInCache)
        {
            tryToRetrieveItemFoundInCache = FoundInCache ? ItemFoundInCache : default;

            return FoundInCache;
        }
    }

    #endregion

}
