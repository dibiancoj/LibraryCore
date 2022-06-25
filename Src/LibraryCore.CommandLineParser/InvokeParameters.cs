using LibraryCore.CommandLineParser.Options;
using System.Collections.Immutable;

namespace LibraryCore.CommandLineParser;

public record InvokeParameters
{
    public IImmutableList<CommandConfiguration> ConfiguredCommands { get; init; } = null!;
    public IDictionary<string, string> RequiredParameters { private get; init; } = null!;
    public Action<string> MessagePump { get; init; } = null!;

    public T RequiredParameterToValue<T>(string key) => (T)Convert.ChangeType(RequiredParameters[key], typeof(T));
}
