using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibraryCore.Core.ExtensionMethods
{
    /// <summary>
    /// Extension methods to help with serialization with a distributed cache. This handles all the serialization using the new json serializer.
    /// </summary>
    public static class DistributedMemoryCacheExtensionMethods
    {

        #region Public Methods

        public static async ValueTask<TItem> GetOrCreateWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, Func<TItem> factory)
        {
            return await distributedCache.GetOrCreateWithJsonSerializerAsync(key, factory, null);
        }

        public static async ValueTask<TItem> GetOrCreateWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, Func<TItem> factory, DistributedCacheEntryOptions? distributedCacheEntryOptions)
        {
            var tryToFindInDistributedCache = await distributedCache.GetAsync(key);

            //we have it just return it
            if (tryToFindInDistributedCache != null)
            {
                return DeserializeFromBytes<TItem>(tryToFindInDistributedCache);
            }

            //we don't have it...lets go add it
            var itemToAdd = factory();

            //didn't find it...go get it
            await distributedCache.SetWithJsonSerializerAsync(key, itemToAdd, distributedCacheEntryOptions);

            return itemToAdd;
        }

        public static async ValueTask SetWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, TItem itemToAdd)
        {
            await distributedCache.SetWithJsonSerializerAsync(key, itemToAdd, null);
        }

        public static async ValueTask SetWithJsonSerializerAsync<TItem>(this IDistributedCache distributedCache, string key, TItem itemToAdd, DistributedCacheEntryOptions? distributedCacheEntryOptions)
        {
            //can't pass null for the distributed options
            await distributedCache.SetAsync(key, SerializeToBytes(itemToAdd), distributedCacheEntryOptions ?? new DistributedCacheEntryOptions());
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

        #endregion

    }
}
