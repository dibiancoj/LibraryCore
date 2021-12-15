namespace LibraryCore.Core.ExtensionMethods;

public static class HttpContentExtensionMethods
{
    /// <summary>
    /// Helper method on an http client response. Allow to grab a xml document from a response.
    /// </summary>
    /// <typeparam name="T">Type to deserialize into</typeparam>
    /// <param name="httpContent">Http content that was received from the web request</param>
    /// <returns>The object. Will be null if the xml serialized was from a null object</returns>
    public static async Task<T?> ReadFromXmlAsync<T>(this HttpContent httpContent)
    {
        using var stream = await httpContent.ReadAsStreamAsync();

        return XmlSerialization.XMLSerializationHelper.DeserializeObject<T>(stream);
    }
}

