using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace LibraryCore.Caching;

/* Since redis is not multi threaded you might run into times outs. With high volume apps
   With stack exchange you can allocate x amount of threads for Redis so it doesn't need to wait x number of seconds to spawn more. So first timeout you wait 10 seconds, next timeout you wait 20 seconds, 30 seconds etc. With the change below you have 50 threads before you start to wait when needing threads

   In .net core just add 

   ```ThreadPool.SetMinThreads(Int32, Int32)``` in the start of the application.
   Set something like ```ThreadPool.SetMinThreads(50,50)```

   ## Only use this if you hit timeouts or redis can't keep up with your traffic volume
*/

public class DistributedCacheService(IDistributedCache distributedCache)
{

    #region Properties

    /// <summary>
    /// Holds a lookup for all the cache locks. You can't lock an async method so we use Semaphore. 
    /// </summary>
    private static ConcurrentDictionary<string, SemaphoreSlim> CacheLocksLookup { get; } = new();

    private static TimeSpan DefaultWaitTimeout { get; } = TimeSpan.FromSeconds(30);

    #endregion

    #region Public Methods

    public async Task<TItem> GetOrCreateAsync<TItem>(string key,
                                                     Func<DistributedCacheEntryOptions, Task<TItem>> factory,
                                                     TimeSpan? acquireLockTimeout = default,
                                                     CancellationToken cancellationToken = default)
    {
        var tryToGrabFromCacheResult = await TryToGetValueFromCache<TItem>(key, cancellationToken);

        //we have it just return it
        if (tryToGrabFromCacheResult.TryGetItemFromResult(out var tryToGetItemFromCacheResult))
        {
            return tryToGetItemFromCacheResult;
        }

        //so we couldn't find it on our first shot. we need to take a lock now
        //couldn't grab it...we need to set the lock
        var semaphoreLockToUseForCacheKey = AcquireSemaphoreSlimForCacheKey(key);

        //grab the count in a variable. If = 1...then you are the thread that is causing the blocking.
        //this value is the remaining threads that can enter the lock (we take the lock on the next row).
        //1 = i'm about to take a lock.
        //0 = Someone has a lock already
        int i = semaphoreLockToUseForCacheKey.CurrentCount;

        //take the lock and wait for it
        if (!await semaphoreLockToUseForCacheKey.WaitAsync(acquireLockTimeout ?? DefaultWaitTimeout, cancellationToken))
        {
            throw new TimeoutException("Can't acquire lock in the allocated period.");
        }

        //we acquired the lock succesfully...so we will put the rest in a try catch

        try
        {
            //if we are the thread that is blocking then don't try to fetch it again. If we were waiting because of a another thread...
            //then the value should be in the cache and go fetch it (still verify its in the cache and don't assume because of contention)
            //0 = Someone had a lock and we had to wait for them. So the item will be in the cache already
            if (i == 0)
            {
                //since we took a lock the value might be there from another thread. so try to get it 1 more time. 
                var tryToGrabFromCacheAfterLockResult = await TryToGetValueFromCache<TItem>(key, cancellationToken);

                //check if we have it
                if (tryToGrabFromCacheAfterLockResult.TryGetItemFromResult(out var tryToGetItemFromCacheResultAfterLock))
                {
                    return tryToGetItemFromCacheResultAfterLock;
                }
            }

            var options = new DistributedCacheEntryOptions();

            var itemToSetInCache = await factory(options);

            //didn't find it...go create it, set it in the cache, and return it
            return await SetAsync(key, itemToSetInCache, options, cancellationToken);
        }
        finally
        {
            //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
            //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
            semaphoreLockToUseForCacheKey.Release();
        }
    }

    public async Task<TItem> SetAsync<TItem>(string key, TItem itemToAdd, CancellationToken cancellationToken = default)
    {
        return await SetAsync(key, itemToAdd, new DistributedCacheEntryOptions(), cancellationToken: cancellationToken);
    }

    public async Task<TItem> SetAsync<TItem>(string key, TItem itemToAdd, DistributedCacheEntryOptions distributedCacheEntryOptions, CancellationToken cancellationToken = default)
    {
        //can't pass null for the distributed options
        await distributedCache.SetAsync(key, SerializeToBytes(itemToAdd), distributedCacheEntryOptions, token: cancellationToken);

        return itemToAdd;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => distributedCache.RemoveAsync(key, token: cancellationToken);

    #endregion

    #region Private Helper Methods

    private async Task<TryToGetInCacheResult<TItem>> TryToGetValueFromCache<TItem>(string key, CancellationToken cancellationToken)
    {
        var tryToFindInDistributedCache = await distributedCache.GetAsync(key, token: cancellationToken);

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

    private static SemaphoreSlim AcquireSemaphoreSlimForCacheKey(string key) => CacheLocksLookup.GetOrAdd(key, (x) => new SemaphoreSlim(1, 1));

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
