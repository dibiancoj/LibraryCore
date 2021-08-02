using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Concurrent;
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
            var tryToFindInDistributedCache = await distributedCache.GetAsync(key);

            //we have it just return it
            if (tryToFindInDistributedCache != null)
            {
                return DeserializeFromBytes<TItem>(tryToFindInDistributedCache);
            }

            //so we couldn't find it on our first shot. we need to take a lock now
            //couldn't grab it...we need to set the lock
            var asyncLockToUse = AcquireLock(key);

            try
            {
                //take the lock and wait for it
                await asyncLockToUse.WaitAsync();

                //we don't have it...lets go add it
                var itemToAdd = await factory();

                //didn't find it...go get it
                await distributedCache.SetWithJsonSerializerAsync(key, itemToAdd, distributedCacheEntryOptions);

                //return the item
                return itemToAdd;
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                asyncLockToUse.Release();
            }
        }

        public static async Task SetWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, TItem itemToAdd)
        {
            await distributedCache.SetWithJsonSerializerAsync(key, itemToAdd, new DistributedCacheEntryOptions());
        }

        public static async Task SetWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, TItem itemToAdd, DistributedCacheEntryOptions distributedCacheEntryOptions)
        {
            //can't pass null for the distributed options
            await distributedCache.SetAsync(key, SerializeToBytes(itemToAdd), distributedCacheEntryOptions);
        }

        #endregion

        #region Private Helper Methods

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

    }
}
