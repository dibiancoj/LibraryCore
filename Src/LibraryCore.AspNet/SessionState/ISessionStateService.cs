namespace LibraryCore.AspNet.SessionState;

public interface ISessionStateService
{
    ValueTask<T?> GetObjectAsync<T>(string key, bool useJsonNetSerializer = false);
    ValueTask<bool> HasKeyInSessionAsync(string key);
    ValueTask<TryToGetResult<T>> TryGetObjectAsync<T>(string key, bool useJsonNetSerializer = false);
    ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> creator, bool useJsonNetSerializer = false);
    ValueTask SetObjectAsync<T>(string key, T objectToPutInSession, bool useJsonNetSerializer = false);
    ValueTask RemoveObjectAsync(string key);
    ValueTask ClearAllSessionObjectsForThisUserAsync();
    ValueTask<IEnumerable<string>> SessionItemKeysAsync();
}
