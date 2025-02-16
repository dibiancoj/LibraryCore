namespace LibraryCore.AspNet.DistributedSessionState;

public interface ISessionStateService
{
    public ValueTask<T?> GetAsync<T>(string key, bool useJsonNetSerializer = false);
    public ValueTask<bool> HasKeyAsync(string key);
    public ValueTask<TryToGetResult<T>> TryGetAsync<T>(string key, bool useJsonNetSerializer = false);
    public ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> creator, bool useJsonNetSerializer = false);
    public ValueTask SetAsync<T>(string key, T objectToPutInSession, bool useJsonNetSerializer = false);
    public ValueTask RemoveAsync(string key);
    public ValueTask ClearAllForUserAsync();
    public ValueTask<IEnumerable<string>> AllKeysAsync();
}
