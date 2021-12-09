using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibraryCore.JsonNet;

public static class JsonNetUtilities
{

    #region Deserialize

    /// <summary>
    /// Deserializes an item from a stream to an object. We don't need to allocate the string that gets thrown away. For larger http responses this is important bc GC doesn't run as often.
    /// </summary>
    /// <typeparam name="T">Type to deserialize</typeparam>
    /// <param name="streamToReadFrom">Stream to read from</param>
    /// <returns>the deserialized object</returns>
    public static T DeserializeFromStream<T>(Stream streamToReadFrom)
    {
        //this is great if you make a web request and you get a stream back.
        return DeserializeFromStream<T>(streamToReadFrom, JsonSerializer.CreateDefault());
    }

    public static T DeserializeFromStream<T>(Stream streamToReadFrom, JsonSerializer jsonSerializer)
    {
        //contains duplicate code to the other untyped method but we want this typed.
        using var streamReaderToUse = new StreamReader(streamToReadFrom);
        using var jsonReaderToUse = new JsonTextReader(streamReaderToUse);

        return jsonSerializer.Deserialize<T>(jsonReaderToUse) ?? throw new Exception("Not Able To Deserialize The Item");
    }

    /// <summary>
    /// Deserializes an item from a stream to an object. We don't need to allocate the string that gets thrown away. For larger http responses this is important bc GC doesn't run as often.
    /// </summary>
    /// <param name="type">Type to deserialize</param>
    /// <param name="streamToReadFrom">Stream to read from</param>
    /// <param name="jsonSerializer">Json serializer settings</param>
    /// <returns>the deserialized object</returns>
    public static object DeserializeFromStream(Type type, Stream streamToReadFrom, JsonSerializer jsonSerializer)
    {
        //this is great if you make a web request and you get a stream back.
        using var streamReaderToUse = new StreamReader(streamToReadFrom);
        using var jsonReaderToUse = new JsonTextReader(streamReaderToUse);

        //read the json from a stream - json size doesn't matter because only a small piece is read at a time from the HTTP request
        return jsonSerializer.Deserialize(jsonReaderToUse, type)!;
    }

    public static T DeserializeFromByteArray<T>(byte[] bytesToDeserialize, JsonSerializer jsonSerializer)
    {
        using var memoryStream = new MemoryStream(bytesToDeserialize);

        return DeserializeFromStream<T>(memoryStream, jsonSerializer);
    }

    #endregion

    #region Serialize

    /// <summary>
    /// Serialize a model straight to a stream.
    /// </summary>
    /// <typeparam name="T">Type of the model to serialize</typeparam>
    /// <param name="modelToSerialize">model to serialize</param>
    /// <param name="jsonSerializer">Serializer if you have special settings</param>
    /// <returns>Stream to consume</returns>
    /// <remarks>Be sure to dispose of the stream when done by the calling code</remarks>
    public static MemoryStream SerializeToStream<T>(T modelToSerialize, JsonSerializer jsonSerializer)
    {
        var memoryStream = new MemoryStream();

        using var streamWriter = new StreamWriter(memoryStream);
        using var jsonWriter = new JsonTextWriter(streamWriter);

        jsonSerializer.Serialize(jsonWriter, modelToSerialize);
        jsonWriter.Flush();
        streamWriter.Flush();

        return memoryStream;
    }

    public static byte[] SerializeToUtf8Bytes<T>(T modelToSerialize, JsonSerializer jsonSerializer)
    {
        using var memoryStream = SerializeToStream(modelToSerialize, jsonSerializer);

        return memoryStream.ToArray();
    }

    #endregion

    #region JObject

    /// <summary>
    /// Create a jobject from a stream. We don't need to allocate the string that gets thrown away. For larger http responses this is important bc GC doesn't run as often.
    /// </summary>
    /// <param name="streamToReadFrom">Stream to read from</param>
    /// <returns>Jobject</returns>
    public static async Task<JObject> JObjectFromStreamAsync(Stream streamToReadFrom)
    {
        using var streamReaderToUse = new StreamReader(streamToReadFrom);
        using var jsonReaderToUse = new JsonTextReader(streamReaderToUse);

        return await JObject.LoadAsync(jsonReaderToUse).ConfigureAwait(false);
    }

    #endregion

}
