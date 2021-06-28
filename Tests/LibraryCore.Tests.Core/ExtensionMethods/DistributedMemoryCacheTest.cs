using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Tests.Core.GlobalMocks;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods
{
    public class DistributedMemoryCacheTest
    {

        #region Get Or Create

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task GetOrCreateCantFindInCache(bool addOptions)
        {
            var key = Guid.NewGuid().ToString();
            var distributedCacheToTestWith = new FullMockIDistributedCache();

            var result = addOptions ?
                            await distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync(key, () => new List<int> { 1, 2, 3 }, new DistributedCacheEntryOptions()) :
                            await distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync(key, () => new List<int> { 1, 2, 3 });

            Assert.Equal(3, result.Count);
            Assert.Contains(result, x => x == 1);
            Assert.Contains(result, x => x == 2);
            Assert.Contains(result, x => x == 3);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task GetOrCreateIsInCache(bool addOptions)
        {
            var key = Guid.NewGuid().ToString();
            var distributedCacheToTestWith = new FullMockIDistributedCache();

            await distributedCacheToTestWith.SetWithJsonSerializerAsync(key, new List<int> { 1, 2, 3 });

            var result = addOptions ?
                                await distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync<List<int>>(key, () => throw new Exception("Should Grab From Cache"), new DistributedCacheEntryOptions()) :
                                await distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync<List<int>>(key, () => throw new Exception("Should Grab From Cache"), null);

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
            var distributedCacheToTestWith = new FullMockIDistributedCache();

            var data = new List<int> { 1, 2, 3 };

            if (addOptions)
            {
                await distributedCacheToTestWith.SetWithJsonSerializerAsync(key, data, new DistributedCacheEntryOptions());
            }
            else
            {
                await distributedCacheToTestWith.SetWithJsonSerializerAsync(key, data);
            }

            var grabItemFromCache = await distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync<List<int>>(key, () => throw new Exception("Should Grab From Cache"));

            Assert.Equal(3, grabItemFromCache.Count);
            Assert.Contains(grabItemFromCache, x => x == 1);
            Assert.Contains(grabItemFromCache, x => x == 2);
            Assert.Contains(grabItemFromCache, x => x == 3);
        }

        #endregion

    }
}
