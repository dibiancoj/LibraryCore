using Confluent.Kafka;
using LibraryCore.Kafka.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace LibraryCore.Kafka.Serialization;

/// <summary>
/// A serializer and deserializer for Kafka messages that handles nullable message payloads.
/// </summary>
/// <typeparam name="T">The type of the message payload.</typeparam>
public class NullableKafkaMessageSerializer<T> : IDeserializer<KafkaNullableOfT<T>>, ISerializer<T>
{
    /// <summary>
    /// Don't want to keep initing over and over. Microsoft says to re-use
    /// </summary>
    private static JsonSerializerOptions Settings { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };


    //have no control over kafka's contract , so we need to suppress the warnings
#pragma warning disable IL3051, IL2046

    /// <summary>
    /// Deserializes the Kafka message payload from a byte span to a <see cref="KafkaNullableOfT{T}"/> object.
    /// </summary>
    /// <param name="data">The read-only span of bytes representing the serialized message payload.</param>
    /// <param name="isNull">A value indicating whether the data is null.</param>
    /// <param name="context">The serialization context provided by Kafka.</param>
    /// <returns>A <see cref="KafkaNullableOfT{T}"/> object representing the deserialized message payload.</returns>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode(Shared.ErrorMessages.AotDynamicAccess)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(Shared.ErrorMessages.AotDynamicAccess)]
#endif
    public KafkaNullableOfT<T> Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(data, Settings);

            return KafkaNullableOfT<T>.SuccessfulMessageSchema(result);
        }
        catch (Exception)
        {
            return KafkaNullableOfT<T>.NotSuccessfulMessageSchema();
        }
    }

    /// <summary>
    /// Serializes the message payload of type <typeparamref name="T"/> to a byte array.
    /// </summary>
    /// <param name="data">The message payload to serialize.</param>
    /// <param name="context">The serialization context provided by Kafka.</param>
    /// <returns>A byte array representing the serialized message payload.</returns>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode(Shared.ErrorMessages.AotDynamicAccess)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(Shared.ErrorMessages.AotDynamicAccess)]
#endif
    public byte[] Serialize(T data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }

#pragma warning restore IL3051, IL2046
}

