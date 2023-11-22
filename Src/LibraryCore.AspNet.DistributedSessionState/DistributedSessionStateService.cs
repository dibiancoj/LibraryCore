using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Text.Json;

namespace LibraryCore.AspNet.DistributedSessionState;

public class DistributedSessionStateService(IHttpContextAccessor httpContextAccessor, JsonSerializerOptions? jsonSerializationOptions) : ISessionStateService
{
    public DistributedSessionStateService(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor, null)
    {
    }

    private ConcurrentDictionary<string, Type> CachedAutoTypeLookup { get; } = new ConcurrentDictionary<string, Type>();

    public async ValueTask<TryToGetResult<T>> TryGetObjectAsync<T>(string key, bool useAutoTypeHandling = false)
    {
        await ResolveHttpContextOrThrow().Session.LoadAsync();

        var foundInSession = ResolveHttpContextOrThrow().Session.TryGetValue(key, out var foundBytes);

        var objectFound = foundInSession && foundBytes != null ?
                            DeserializeItem<T>(foundBytes, useAutoTypeHandling) :
                            default;

        return new TryToGetResult<T>(foundInSession, objectFound);
    }

    public async ValueTask<T?> GetObjectAsync<T>(string key, bool useAutoTypeHandling = false)
    {
        var result = await TryGetObjectAsync<T>(key, useAutoTypeHandling);

        _ = result.GetItemIfFoundInSession(out var itemInSession);

        return itemInSession;
    }

    public async ValueTask SetObjectAsync<T>(string key, T objectToPutInSession, bool useAutoTypeHandling = false)
    {
        await ResolveHttpContextOrThrow().Session.LoadAsync();

        ResolveHttpContextOrThrow().Session.Set(key, SerializeObject(objectToPutInSession, useAutoTypeHandling));
        await ResolveHttpContextOrThrow().Session.CommitAsync();
    }

    public async ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> creator, bool useAutoTypeHandling = false)
    {
        var result = await TryGetObjectAsync<T>(key, useAutoTypeHandling);

        if (result.GetItemIfFoundInSession(out var itemInSession))
        {
            //found it right away return
            return itemInSession;
        }

        //not found in session go create it, set it, and rturn it
        var objectToPutInSession = await creator();

        //set the object
        await SetObjectAsync(key, objectToPutInSession, useAutoTypeHandling);

        return objectToPutInSession;
    }

    public async ValueTask RemoveObjectAsync(string key)
    {
        await ResolveHttpContextOrThrow().Session.LoadAsync();

        ResolveHttpContextOrThrow().Session.Remove(key);
        await ResolveHttpContextOrThrow().Session.CommitAsync();
    }

    public async ValueTask ClearAllSessionObjectsForThisUserAsync()
    {
        await ResolveHttpContextOrThrow().Session.LoadAsync();

        ResolveHttpContextOrThrow().Session.Clear();
        await ResolveHttpContextOrThrow().Session.CommitAsync();
    }

    public async ValueTask<bool> HasKeyInSessionAsync(string key) => (await SessionItemKeysAsync()).Contains(key);

    public async ValueTask<IEnumerable<string>> SessionItemKeysAsync()
    {
        await ResolveHttpContextOrThrow().Session.LoadAsync();

        return ResolveHttpContextOrThrow().Session.Keys;
    }

    private byte[] SerializeObject<T>(T objectToPutInSession, bool useAutoTypeHandling)
    {
        return useAutoTypeHandling ?

                    JsonSerializer.SerializeToUtf8Bytes(new AutoTypeHandling
                    (
                        typeof(T).AssemblyQualifiedName ?? throw new Exception("Assembly Qualified Name Null"),
                        JsonSerializer.SerializeToUtf8Bytes(objectToPutInSession, options: jsonSerializationOptions)
                    ), options: jsonSerializationOptions) :

                    JsonSerializer.SerializeToUtf8Bytes(objectToPutInSession, options: jsonSerializationOptions);
    }

    private T? DeserializeItem<T>(byte[] bytesToDeserialize, bool useAutoTypeHandling)
    {
        //regular serialization and not an interface or derived class
        if (!useAutoTypeHandling)
        {
            return JsonSerializer.Deserialize<T>(bytesToDeserialize, jsonSerializationOptions);
        }

        var temp = JsonSerializer.Deserialize<AutoTypeHandling>(bytesToDeserialize, jsonSerializationOptions) ?? throw new Exception("Temp should never be null here even with a null value that got serialized. Null will serialize 'null' string");

        Type typeToDeserialize = CachedAutoTypeLookup.GetOrAdd(temp.FullTypePath, (path) => Type.GetType(path) ?? throw new Exception("Type Not Found: " + temp.FullTypePath));

        var tempBeforeCast = JsonSerializer.Deserialize(temp.ValueInBytes, typeToDeserialize, jsonSerializationOptions);

        return tempBeforeCast == null ?
                    default :
                    (T)tempBeforeCast;
    }

    private HttpContext ResolveHttpContextOrThrow() => httpContextAccessor.HttpContext ?? throw new NullReferenceException("HttpContext Not Found In Accessor");

    private record AutoTypeHandling(string FullTypePath, byte[] ValueInBytes);

}
