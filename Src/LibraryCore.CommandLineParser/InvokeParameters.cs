using LibraryCore.CommandLineParser.Options;
using System.Collections.Immutable;

namespace LibraryCore.CommandLineParser;

public record InvokeParameters
{
    public IImmutableList<CommandConfiguration> ConfiguredCommands { get; init; } = null!;
    public IImmutableDictionary<string, string> RequiredArguments { private get; init; } = null!;
    public Action<string> MessagePump { get; init; } = null!;

    public T RequiredParameterToValue<T>(string key) => (T)Convert.ChangeType(RequiredArguments[key], typeof(T));
}
