using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Text.Json;

namespace LibraryCore.AspNet.SessionState;

public class DistributedSessionStateService : ISessionStateService
{
    public DistributedSessionStateService(IHttpContextAccessor httpContextAccessor, JsonSerializerOptions? jsonSerializationOptions)
    {
        HttpContextAccessor = httpContextAccessor;
        JsonSerializationOption = jsonSerializationOptions;
        CachedAutoTypeLookup = new ConcurrentDictionary<string, Type>();
    }

    public DistributedSessionStateService(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor, null)
    {
    }

    private IHttpContextAccessor HttpContextAccessor { get; }
    private JsonSerializerOptions? JsonSerializationOption { get; }
    private ConcurrentDictionary<string, Type> CachedAutoTypeLookup { get; }

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
                    {
                        FullTypePath = typeof(T).AssemblyQualifiedName ?? throw new Exception("Assembly Qualified Name Null"),
                        ValueInBytes = JsonSerializer.SerializeToUtf8Bytes(objectToPutInSession, options: JsonSerializationOption)
                    }, options: JsonSerializationOption) :

                    JsonSerializer.SerializeToUtf8Bytes(objectToPutInSession, options: JsonSerializationOption);
    }

    private T? DeserializeItem<T>(byte[] bytesToDeserialize, bool useAutoTypeHandling)
    {
        if (useAutoTypeHandling)
        {
            var temp = JsonSerializer.Deserialize<AutoTypeHandling>(bytesToDeserialize, JsonSerializationOption) ?? throw new Exception("Temp should never be null here even with a null value that got serialized. Null will serialize 'null' string");

            Type typeToDeserialize = CachedAutoTypeLookup.GetOrAdd(temp.FullTypePath, (path) => Type.GetType(path) ?? throw new Exception("Type Not Found: " + temp.FullTypePath));

            var tempBeforeCast = JsonSerializer.Deserialize(temp.ValueInBytes, typeToDeserialize, JsonSerializationOption);

            return tempBeforeCast == null ?
                        default :
                        (T)tempBeforeCast;
        }

        return JsonSerializer.Deserialize<T>(bytesToDeserialize, JsonSerializationOption);
    }

    private HttpContext ResolveHttpContextOrThrow() => HttpContextAccessor.HttpContext ?? throw new NullReferenceException("HttpContext Not Found In Accessor");

    private class AutoTypeHandling
    {
        public string FullTypePath { get; set; } = null!;
        public byte[] ValueInBytes { get; set; } = default!;
    }

}
