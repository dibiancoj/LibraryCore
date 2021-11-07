using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryCore.Tests.Core.GlobalMocks
{
    public class FullMockIDistributedCache : IDistributedCache
    {
        private ConcurrentDictionary<string, byte[]> CacheStore { get; } = new();

        public byte[] Get(string key) => CacheStore.GetValueOrDefault(key);

        public Task<byte[]> GetAsync(string key, CancellationToken token = default) => Task.FromResult(CacheStore.GetValueOrDefault(key));

        public void Refresh(string key) => throw new NotImplementedException();

        public Task RefreshAsync(string key, CancellationToken token = default) => throw new NotImplementedException();

        public void Remove(string key) => CacheStore.TryRemove(key, out _);

        public Task RemoveAsync(string key, CancellationToken token = default) => Task.FromResult(CacheStore.TryRemove(key, out _));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => CacheStore.TryAdd(key, value);

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            CacheStore[key] = value;

            return Task.CompletedTask;
        }
    }
}
