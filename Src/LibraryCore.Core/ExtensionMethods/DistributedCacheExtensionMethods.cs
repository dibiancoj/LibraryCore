using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryCore.Core.ExtensionMethods
{
    /// <summary>
    /// Extension methods to help with serialization with a distributed cache. This handles all the serialization using the new json serializer.
    /// </summary>
    public static class DistributedCacheExtensionMethods
    {

        /// <summary>
        /// Holds a lookup for all the cache locks. You can't lock an async method so we use Semaphore. 
        /// </summary>
        private static ConcurrentDictionary<object, SemaphoreSlim> CacheLocksLookup { get; } = new ConcurrentDictionary<object, SemaphoreSlim>();

        #region Public Methods

        public static async Task<TItem> GetOrCreateWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, Func<Task<TItem>> factory)
        {
            return await distributedCache.GetOrCreateWithJsonSerializerAsync(key, factory, new DistributedCacheEntryOptions());
        }

        public static async Task<TItem> GetOrCreateWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, Func<Task<TItem>> factory, DistributedCacheEntryOptions distributedCacheEntryOptions)
        {
            var tryToGrabFromCacheResult = await TryToGetValueFromCache<TItem>(distributedCache, key);

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
                    var tryToGrabFromCacheAfterLockResult = await TryToGetValueFromCache<TItem>(distributedCache, key);

                    //check if we have it
                    if (tryToGrabFromCacheAfterLockResult.TryGetItemFromResult(out var tryToGetItemFromCacheResultAfterLock))
                    {
                        return tryToGetItemFromCacheResultAfterLock;
                    }
                }

                //didn't find it...go create it, set it in the cache, and return it
                return await distributedCache.SetWithJsonSerializerAsync(key, await factory(), distributedCacheEntryOptions);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                asyncLockToUse.Release();
            }
        }

        public static async Task<TItem> SetWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, TItem itemToAdd)
        {
            return await distributedCache.SetWithJsonSerializerAsync(key, itemToAdd, new DistributedCacheEntryOptions());
        }

        public static async Task<TItem> SetWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, TItem itemToAdd, DistributedCacheEntryOptions distributedCacheEntryOptions)
        {
            //can't pass null for the distributed options
            await distributedCache.SetAsync(key, SerializeToBytes(itemToAdd), distributedCacheEntryOptions);

            return itemToAdd;
        }

        #endregion

        #region Private Helper Methods

        private static async Task<TryToGetInCacheResult<TItem>> TryToGetValueFromCache<TItem>(IDistributedCache distributedCache, string key)
        {
            var tryToFindInDistributedCache = await distributedCache.GetAsync(key);

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
        private static T DeserializeFromBytes<T>(byte[] bytes) => JsonSerializer.Deserialize<T>(bytes) ?? throw new Exception("Not Able To Deserialize Bytes");

        private static SemaphoreSlim AcquireLock(object key)
        {
            //try to grab the lock
            if (CacheLocksLookup.TryGetValue(key, out var lockAttempt))
            {
                //found it in the dictionary
                return lockAttempt;
            }

            //create the new lock
            lockAttempt = new SemaphoreSlim(1, 1);

            //add it to the dictionary
            CacheLocksLookup.TryAdd(key, lockAttempt);

            //return the lock
            return lockAttempt;
        }

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
}
