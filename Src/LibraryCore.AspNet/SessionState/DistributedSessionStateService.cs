using LibraryCore.Core.ExtensionMethods;
using LibraryCore.JsonNet;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibraryCore.AspNet.SessionState
{
    public class DistributedSessionStateService : ISessionStateService
    {
        public DistributedSessionStateService(IHttpContextAccessor httpContextAccessor, IEnumerable<System.Text.Json.Serialization.JsonConverter> jsonTextConverters)
        {
            HttpContextAccessor = httpContextAccessor;
            CachedJsonSerializerOptions = new JsonSerializerOptions();
            JsonNetSerializerOptions = new Newtonsoft.Json.JsonSerializer { TypeNameHandling = TypeNameHandling.All };
            jsonTextConverters.ForEach(x => CachedJsonSerializerOptions.Converters.Add(x));
        }

        private IHttpContextAccessor HttpContextAccessor { get; }
        private JsonSerializerOptions CachedJsonSerializerOptions { get; }
        private Newtonsoft.Json.JsonSerializer JsonNetSerializerOptions { get; }

        public async ValueTask<(bool ItemFoundInSession, T ItemInSession)> TryGetObjectAsync<T>(string key, bool useJsonNetSerializer = false)
        {
            await HttpContextAccessor.HttpContext.Session.LoadAsync().ConfigureAwait(false);

            var foundInSession = HttpContextAccessor.HttpContext.Session.TryGetValue(key, out var foundBytes);

            T? DeserializeItem(byte[] bytes)
            {
                return useJsonNetSerializer ?
                        JsonNetUtilities.DeserializeFromByteArray<T>(bytes, JsonNetSerializerOptions) :
                        System.Text.Json.JsonSerializer.Deserialize<T>(bytes, CachedJsonSerializerOptions);
            }

            var objectFound = foundInSession ?
                                DeserializeItem(foundBytes) :
                                default;

            return (foundInSession, objectFound!); //with new vs throws nullability (as this differs from contract). Leaving for now since the boolean should be checked first.
        }

        public async ValueTask<T> GetObjectAsync<T>(string key, bool useJsonNetSerializer = false)
        {
            //we don't care if it fails or not since the failure will return the default value
            return (await TryGetObjectAsync<T>(key, useJsonNetSerializer).ConfigureAwait(false)).ItemInSession;
        }

        public async ValueTask SetObjectAsync<T>(string key, T objectToPutInSession, bool useJsonNetSerializer = false)
        {
            await HttpContextAccessor.HttpContext.Session.LoadAsync().ConfigureAwait(false);

            byte[] serializedBytes = useJsonNetSerializer ?
                                        JsonNetUtilities.SerializeToUtf8Bytes(objectToPutInSession, JsonNetSerializerOptions) :
                                        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(objectToPutInSession);

            HttpContextAccessor.HttpContext.Session.Set(key, serializedBytes);
            await HttpContextAccessor.HttpContext.Session.CommitAsync().ConfigureAwait(false);
        }

        public async ValueTask<T> GetOrSetAsync<T>(string key, Func<Task<T>> creator, bool useJsonNetSerializer = false)
        {
            var tryToGetValue = await TryGetObjectAsync<T>(key, useJsonNetSerializer);

            if (tryToGetValue.ItemFoundInSession)
            {
                //found it right away return
                return tryToGetValue.ItemInSession;
            }

            //not found in session go create it, set it, and rturn it
            var objectToPutInSession = await creator().ConfigureAwait(false);

            //set the object
            await SetObjectAsync(key, objectToPutInSession, useJsonNetSerializer);

            return objectToPutInSession;
        }

        public async ValueTask RemoveObjectAsync(string key)
        {
            await HttpContextAccessor.HttpContext.Session.LoadAsync().ConfigureAwait(false);

            HttpContextAccessor.HttpContext.Session.Remove(key);
            await HttpContextAccessor.HttpContext.Session.CommitAsync().ConfigureAwait(false);
        }

        public async ValueTask ClearAllSessionObjectsForThisUserAsync()
        {
            await HttpContextAccessor.HttpContext.Session.LoadAsync().ConfigureAwait(false);

            HttpContextAccessor.HttpContext.Session.Clear();
            await HttpContextAccessor.HttpContext.Session.CommitAsync().ConfigureAwait(false);
        }

        public async ValueTask<bool> HasKeyInSessionAsync(string key) => (await SessionItemKeysAsync()).Contains(key);

        public async ValueTask<IEnumerable<string>> SessionItemKeysAsync()
        {
            await HttpContextAccessor.HttpContext.Session.LoadAsync().ConfigureAwait(false);

            return HttpContextAccessor.HttpContext.Session.Keys;
        }

    }
}
