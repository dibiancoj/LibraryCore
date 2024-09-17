using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Kafka.Models;

/// <summary>
/// A messaage schema that can be used to represent a message that was deserialized successfully or not.
/// </summary>
/// <typeparam name="T">The type of message being received from Kafka</typeparam>
public class KafkaNullableOfT<T>
{
    /// <summary>
    /// Prevent constructors from being called outside of this class
    /// </summary>
    private KafkaNullableOfT() { }

    private T? Value { get; set; } = default;
    public bool WasDeserialized { get; private set; }

    /// <summary>
    /// create a new instance was deserialized was successful.
    /// </summary>
    /// <param name="model">The model of the deserialized item</param>
    /// <returns>New instance</returns>
    public static KafkaNullableOfT<T> SuccessfulMessageSchema(T? model)
    {
        //going to be opinionated and say that if a model is null, we will return a not successful message schema. This way we can parse and ensure its not null when it was deserialized.
        return model is null ?
            NotSuccessfulMessageSchema() :
            new KafkaNullableOfT<T> { WasDeserialized = true, Value = model };
    }

    /// <summary>
    /// create a new instance when deserialized was not successful.
    /// </summary>
    /// <returns>New instance</returns>
    public static KafkaNullableOfT<T> NotSuccessfulMessageSchema()
    {
        return new KafkaNullableOfT<T> { WasDeserialized = false };
    }

    /// <summary>
    /// Was the message deserialized successfully. If yes the out parameter will have the value.
    /// </summary>
    /// <param name="tryGetResult">the value of the model if deserialized successfully</param>
    /// <returns>If the value was deserialized correctly</returns>
    public bool TryGetResult([NotNullWhen(true)] out T? tryGetResult)
    {
        tryGetResult = Value;

        return WasDeserialized;
    }

}