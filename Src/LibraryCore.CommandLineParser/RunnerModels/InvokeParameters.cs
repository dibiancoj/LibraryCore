using LibraryCore.CommandLineParser.Options;
using System.Collections.Immutable;

namespace LibraryCore.CommandLineParser.RunnerModels;

public record InvokeParameters
{
    public IImmutableList<CommandConfiguration> ConfiguredCommands { get; init; } = null!;
    public IImmutableDictionary<string, string> RequiredArguments { get; init; } = null!;
    public IImmutableDictionary<string, string?> OptionalArguments { get; init; } = null!;
    public Action<string> MessagePump { get; init; } = null!;

    public T RequiredParameterToValue<T>(string key) => (T)Convert.ChangeType(RequiredArguments[key], typeof(T));
    public (bool IsSpecified, T? ValueIfSpecified) OptionalParameterToValue<T>(string key)
    {
        if (OptionalArguments.TryGetValue(key, out var tryToGetValue))
        {
            //null if a command after argument is not required
            return string.IsNullOrEmpty(tryToGetValue) ?
                                       (true, default(T)) :
                                       (true, (T)Convert.ChangeType(tryToGetValue, typeof(T)));
        }

        return (false, default(T));
    }
}
