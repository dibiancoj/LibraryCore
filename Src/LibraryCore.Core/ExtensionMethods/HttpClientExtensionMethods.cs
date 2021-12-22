using System.Net.Http.Json;

namespace LibraryCore.Core.ExtensionMethods;

public static class HttpClientExtensionMethods
{
    public static async Task<T?> SendRequestToJsonAsync<T>(this HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        var rawResponse = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        return await rawResponse.EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }
}
