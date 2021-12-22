using LibraryCore.JsonNet;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text.Json;

namespace LibraryCore.AspNet.SessionState;

public class DistributedSessionStateService : ISessionStateService
{
    public DistributedSessionStateService(IHttpContextAccessor httpContextAccessor, IEnumerable<System.Text.Json.Serialization.JsonConverter>? jsonTextConverters)
    {
        HttpContextAccessor = httpContextAccessor;
        CachedJsonSerializerOptions = CreateSystemTextJsonSerializationOptions(jsonTextConverters);
        JsonNetSerializerOptions = new Newtonsoft.Json.JsonSerializer { TypeNameHandling = TypeNameHandling.All };
    }

    private IHttpContextAccessor HttpContextAccessor { get; }
    private JsonSerializerOptions CachedJsonSerializerOptions { get; }
    private Newtonsoft.Json.JsonSerializer JsonNetSerializerOptions { get; }

    public async ValueTask<TryToGetResult<T>> TryGetObjectAsync<T>(string key, bool useJsonNetSerializer = false)
    {
        await HttpContextAccessor.HttpContext.Session.LoadAsync();

        var foundInSession = HttpContextAccessor.HttpContext.Session.TryGetValue(key, out var foundBytes);

        var objectFound = foundInSession ?
                            DeserializeItem<T>(foundBytes, useJsonNetSerializer) :
                            default;

        return new TryToGetResult<T>(foundInSession, objectFound); //with new vs throws nullability (as this differs from contract). Leaving for now since the boolean should be checked first.
    }

    public async ValueTask<T?> GetObjectAsync<T>(string key, bool useJsonNetSerializer = false)
    {
        var result = await TryGetObjectAsync<T>(key, useJsonNetSerializer);

        _ = result.GetItemIfFoundInSession(out var itemInSession);

        return itemInSession;
    }

    public async ValueTask SetObjectAsync<T>(string key, T objectToPutInSession, bool useJsonNetSerializer = false)
    {
        await HttpContextAccessor.HttpContext.Session.LoadAsync();

        HttpContextAccessor.HttpContext.Session.Set(key, SerializeObject(objectToPutInSession, useJsonNetSerializer));
        await HttpContextAccessor.HttpContext.Session.CommitAsync();
    }

    public async ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> creator, bool useJsonNetSerializer = false)
    {
        var result = await TryGetObjectAsync<T>(key, useJsonNetSerializer);

        if (result.GetItemIfFoundInSession(out var itemInSession))
        {
            //found it right away return
            return itemInSession;
        }

        //not found in session go create it, set it, and rturn it
        var objectToPutInSession = await creator();

        //set the object
        await SetObjectAsync(key, objectToPutInSession, useJsonNetSerializer);

        return objectToPutInSession;
    }

    public async ValueTask RemoveObjectAsync(string key)
    {
        await HttpContextAccessor.HttpContext.Session.LoadAsync();

        HttpContextAccessor.HttpContext.Session.Remove(key);
        await HttpContextAccessor.HttpContext.Session.CommitAsync();
    }

    public async ValueTask ClearAllSessionObjectsForThisUserAsync()
    {
        await HttpContextAccessor.HttpContext.Session.LoadAsync();

        HttpContextAccessor.HttpContext.Session.Clear();
        await HttpContextAccessor.HttpContext.Session.CommitAsync();
    }

    public async ValueTask<bool> HasKeyInSessionAsync(string key) => (await SessionItemKeysAsync()).Contains(key);

    public async ValueTask<IEnumerable<string>> SessionItemKeysAsync()
    {
        await HttpContextAccessor.HttpContext.Session.LoadAsync();

        return HttpContextAccessor.HttpContext.Session.Keys;
    }

    private byte[] SerializeObject<T>(T objectToPutInSession, bool useJsonNetSerializer)
    {
        return useJsonNetSerializer ?
                   JsonNetUtilities.SerializeToUtf8Bytes(objectToPutInSession, JsonNetSerializerOptions) :
                   System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(objectToPutInSession, options: CachedJsonSerializerOptions);
    }

    private T? DeserializeItem<T>(byte[] bytesToDeserialize, bool useJsonNetSerializer)
    {
        return useJsonNetSerializer ?
                    JsonNetUtilities.DeserializeFromByteArray<T>(bytesToDeserialize, JsonNetSerializerOptions) :
                    System.Text.Json.JsonSerializer.Deserialize<T>(bytesToDeserialize, CachedJsonSerializerOptions);
    }

    private static JsonSerializerOptions CreateSystemTextJsonSerializationOptions(IEnumerable<System.Text.Json.Serialization.JsonConverter>? jsonTextConverters)
    {
        var options = new JsonSerializerOptions();

        //don't want to add a dependency on LibraryCore. Will just write this out. This should only get loaded once per app.
        //i wanted to get this verbose code out of the constructor method
        if (jsonTextConverters != null && jsonTextConverters.Any())
        {
            foreach (var textConverter in jsonTextConverters)
            {
                options.Converters.Add(textConverter);
            }
        }

        return options;
    }

}
