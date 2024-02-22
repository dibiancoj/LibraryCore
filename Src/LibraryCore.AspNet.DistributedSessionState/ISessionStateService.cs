namespace LibraryCore.AspNet.DistributedSessionState;

public interface ISessionStateService
{
    ValueTask<T?> GetAsync<T>(string key, bool useJsonNetSerializer = false);
    ValueTask<bool> HasKeyAsync(string key);
    ValueTask<TryToGetResult<T>> TryGetAsync<T>(string key, bool useJsonNetSerializer = false);
    ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> creator, bool useJsonNetSerializer = false);
    ValueTask SetAsync<T>(string key, T objectToPutInSession, bool useJsonNetSerializer = false);
    ValueTask RemoveAsync(string key);
    ValueTask ClearAllForUserAsync();
    ValueTask<IEnumerable<string>> AllKeysAsync();
}
