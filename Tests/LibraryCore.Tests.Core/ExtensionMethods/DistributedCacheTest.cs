using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Tests.Core.GlobalMocks;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods
{
    public class DistributedCacheTest
    {

        #region Get Or Create

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task GetOrCreateInCache(bool addOptions)
        {
            var key = Guid.NewGuid().ToString();
            var distributedCacheToTestWith = new FullMockIDistributedCache();
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
                    return await distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync(key, factoryCall, new DistributedCacheEntryOptions());
                }

                return await distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync(key, factoryCall);
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

        #region 2 Threads With One Grabbing a Lock

        [Fact]
        public async Task LockContention()
        {
            var key = Guid.NewGuid().ToString();
            var distributedCacheToTestWith = new FullMockIDistributedCache();

            //start thread 1 without awaiting it
            var startThread1 = distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync(key, async () =>
            {
                await Task.Delay(3000);

                return 9999;
            });

            //kick off thread 2 which goes right away and should pick up the lock within 3 seconds
            var startThread2 = distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync(key, () => Task.FromResult(1111));

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
            var distributedCacheToTestWith = new FullMockIDistributedCache();

            //set the first value
            await distributedCacheToTestWith.SetWithJsonSerializerAsync("CantDeserializeBytes", "test 123");

            //try to deserialize it to a random class
            await Assert.ThrowsAsync<JsonException>(() => distributedCacheToTestWith.GetOrCreateWithJsonSerializerAsync<CantDeserializeBytesModel>("CantDeserializeBytes", () => throw new NotImplementedException()));
        }

        #endregion

    }
}
