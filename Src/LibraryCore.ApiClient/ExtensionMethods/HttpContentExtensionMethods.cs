using LibraryCore.Shared;
using LibraryCore.XmlSerialization;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.ApiClient.ExtensionMethods;

public static class HttpContentExtensionMethods
{
    /// <summary>
    /// Helper method on an http client response. Allow to grab a xml document from a response.
    /// </summary>
    /// <typeparam name="T">Type to deserialize into</typeparam>
    /// <param name="httpContent">Http content that was received from the web request</param>
    /// <returns>The object. Will be null if the xml serialized was from a null object</returns>
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
    public static async Task<T?> ReadFromXmlAsync<T>(this HttpContent httpContent, CancellationToken cancellationToken = default)
    {
        using var stream = await httpContent.ReadAsStreamAsync(cancellationToken);

        return XMLSerializationHelper.DeserializeObject<T>(stream) ?? throw new Exception("Deserializing A Null Xml Document");
    }
}

