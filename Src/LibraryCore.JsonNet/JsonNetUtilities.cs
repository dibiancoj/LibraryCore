using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibraryCore.JsonNet;

public static class JsonNetUtilities
{
    //** Note **
    //The nullable reference checks has deserialize as nullable
    //Passing in a null stream / string. Or "null"...Will result in a null object.
    //We want to be correct so will return a nullable type as well.

    #region Deserialize

    /// <summary>
    /// Deserializes an item from a stream to an object. We don't need to allocate the string that gets thrown away. For larger http responses this is important bc GC doesn't run as often.
    /// </summary>
    /// <typeparam name="T">Type to deserialize</typeparam>
    /// <param name="streamToReadFrom">Stream to read from</param>
    /// <returns>The deserialized object or a null object if the representation is null</returns>
    public static T? DeserializeFromStream<T>(Stream streamToReadFrom)
    {
        //this is great if you make a web request and you get a stream back.
        return DeserializeFromStream<T>(streamToReadFrom, JsonSerializer.CreateDefault());
    }

    public static T? DeserializeFromStream<T>(Stream streamToReadFrom, JsonSerializer jsonSerializer)
    {
        //contains duplicate code to the other untyped method but we want this typed.
        using var streamReaderToUse = new StreamReader(streamToReadFrom);
        using var jsonReaderToUse = new JsonTextReader(streamReaderToUse);

        return jsonSerializer.Deserialize<T>(jsonReaderToUse);
    }

    /// <summary>
    /// Deserializes an item from a stream to an object. We don't need to allocate the string that gets thrown away. For larger http responses this is important bc GC doesn't run as often.
    /// </summary>
    /// <param name="type">Type to deserialize</param>
    /// <param name="streamToReadFrom">Stream to read from</param>
    /// <param name="jsonSerializer">Json serializer settings</param>
    /// <returns>The deserialized object or a null object if the representation is null</returns>
    public static object? DeserializeFromStream(Type type, Stream streamToReadFrom, JsonSerializer jsonSerializer)
    {
        //this is great if you make a web request and you get a stream back.
        using var streamReaderToUse = new StreamReader(streamToReadFrom);
        using var jsonReaderToUse = new JsonTextReader(streamReaderToUse);

        //read the json from a stream - json size doesn't matter because only a small piece is read at a time from the HTTP request
        return jsonSerializer.Deserialize(jsonReaderToUse, type);
    }

    public static T? DeserializeFromByteArray<T>(byte[] bytesToDeserialize, JsonSerializer jsonSerializer)
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
    /// <returns>Stream to consume. This stream will be closed stream which you can call Encoding.UTF8.GetString(createdStream.ToArray() on</returns>
    /// <remarks>Be sure to dispose of the stream when done by the calling code</remarks>
    public static JsonToStreamResult SerializeToStream<T>(T? modelToSerialize, JsonSerializer jsonSerializer) => JsonToStreamResult.SerializeToStream(modelToSerialize, jsonSerializer);

    public static byte[] SerializeToUtf8Bytes<T>(T? modelToSerialize, JsonSerializer jsonSerializer) => SerializeToStream(modelToSerialize, jsonSerializer).ToByteArray();

    #endregion

    #region JObject

    /// <summary>
    /// Create a jobject from a stream. We don't need to allocate the string that gets thrown away. For larger http responses this is important bc GC doesn't run as often.
    /// </summary>
    /// <param name="streamToReadFrom">Stream to read from</param>
    /// <returns>Jobject</returns>
    public static async Task<JObject> JObjectFromStreamAsync(Stream streamToReadFrom)
    {
        //jobject doesn't support null types / objects. So this will always return an instance or throw (it throws in json.net)
        using var streamReaderToUse = new StreamReader(streamToReadFrom);
        using var jsonReaderToUse = new JsonTextReader(streamReaderToUse);

        return await JObject.LoadAsync(jsonReaderToUse);
    }

    #endregion

}
