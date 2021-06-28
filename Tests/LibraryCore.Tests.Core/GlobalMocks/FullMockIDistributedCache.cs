using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryCore.Tests.Core.GlobalMocks
{
    public class FullMockIDistributedCache : IDistributedCache
    {
        private Dictionary<string, byte[]> CacheStore { get; } = new Dictionary<string, byte[]>();

        public byte[] Get(string key) => CacheStore.TryGetValue(key, out var tryGet) ? tryGet : default;

        public Task<byte[]> GetAsync(string key, CancellationToken token = default) => CacheStore.TryGetValue(key, out var tryGet) ? Task.FromResult(tryGet) : Task.FromResult<byte[]>(default);

        public void Refresh(string key) => throw new NotImplementedException();

        public Task RefreshAsync(string key, CancellationToken token = default) => throw new NotImplementedException();

        public void Remove(string key) => CacheStore.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default) => Task.FromResult(CacheStore.Remove(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => CacheStore[key] = value;

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            CacheStore[key] = value;

            return Task.CompletedTask;
        }
    }
}
