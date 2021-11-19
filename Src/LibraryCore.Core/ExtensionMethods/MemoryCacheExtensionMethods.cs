using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace LibraryCore.Core.ExtensionMethods;

public static class MemoryCacheExtensionMethods
{

    /// <summary>
    /// Holds a lookup for all the cache locks. You can't lock an async method so we use Semaphore. 
    /// </summary>
    private static ConcurrentDictionary<object, SemaphoreSlim> CacheLocksLookup { get; } = new ConcurrentDictionary<object, SemaphoreSlim>();

    /// <summary>
    /// Holds a lock when we go back to the data source so other callers don't all go back to the data source.
    /// </summary>
    /// <returns>Data from either the cache or the data source</returns>
    public static async Task<TItem> GetOrCreateExclusiveAsync<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, Task<TItem>> factory)
    {
        //try to eagerly grab the cache item without taking a lock
        if (cache.TryGetValue<TItem>(key, out var tryToGetItemOptimistic))
        {
            //we have it in our cache...return it
            return tryToGetItemOptimistic;
        }

        //so we couldn't find it on our first shot. we need to take a lock now
        //couldn't grab it...we need to set the lock
        var asyncLockToUse = AcquireLock(key);

        try
        {
            //take the lock and wait for it
            await asyncLockToUse.WaitAsync();

            //if we were waiting for a lock to be released by another thread then most likely its in the cache now. We will try to grab the item again.
            //that is why we are doing a GetOrCreate. this way we try to fetch it on more time before we create it and put it in the cache.
            return await cache.GetOrCreateAsync(key, factory);
        }
        finally
        {
            //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
            //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
            asyncLockToUse.Release();
        }
    }

    /// <summary>
    /// Create a cache item where you can remove that 'group' from the cache using a cancellation token
    /// </summary>
    public static async Task<TItem> GetOrCreateExclusiveWithEvictionAsync<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, Task<TItem>> factory, CancellationTokenSource cancellationToken)
    {
        return await cache.GetOrCreateExclusiveAsync(key, async x =>
        {
            x.AddExpirationToken(new CancellationChangeToken(cancellationToken.Token));

            return await factory(x).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private static SemaphoreSlim AcquireLock(object key) => CacheLocksLookup.GetOrAdd(key, (x) => new SemaphoreSlim(1, 1));

}
