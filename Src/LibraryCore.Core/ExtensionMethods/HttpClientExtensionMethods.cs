using System.Net.Http.Json;

namespace LibraryCore.Core.ExtensionMethods;

public static class HttpClientExtensionMethods
{
    public static async Task<T?> SendRequestToJsonAsync<T>(this HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        var rawResponse = await SendMessageHelper(httpClient, requestMessage, cancellationToken);

        return await rawResponse.EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public static async Task<T?> SendRequestToXmlAsync<T>(this HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        var rawResponse = await SendMessageHelper(httpClient, requestMessage, cancellationToken);

        return await rawResponse.Content.ReadFromXmlAsync<T>(cancellationToken: cancellationToken);
    }

    private static Task<HttpResponseMessage> SendMessageHelper(HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        return httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}
