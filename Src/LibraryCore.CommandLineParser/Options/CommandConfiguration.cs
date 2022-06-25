using System.Collections.Immutable;
using System.Diagnostics;

namespace LibraryCore.CommandLineParser.Options;

[DebuggerDisplay("Command = {CommandName}")]
public class CommandConfiguration
{
    //MyCli [CommandName] [RequiredArgument] -p {optionalArgument}
    //MyCli [CommandName] [RequiredArgument] -v

    //Types of parameters
    //Command Name
    //Required Args
    //Optional Args With No Parameter
    //Optional Args With Parameters

    public CommandConfiguration(string commandName, string commandHelp, Func<InvokeParameters, Task<int>> invoker, OptionsBuilder optionsBuilder)
    {
        RequiredArguments = new List<RequiredArgument>();
        Invoker = invoker;
        OptionsBuilder = optionsBuilder;
        CommandName = commandName;
        CommandHelp = commandHelp;
    }

    public Func<InvokeParameters, Task<int>> Invoker { get; }
    private OptionsBuilder OptionsBuilder { get; }
    public string CommandName { get; }
    public string CommandHelp { get; }
    public List<RequiredArgument> RequiredArguments { get; }
    public int? OrderId { get; private set; }

    //public static CommandConfiguration Create(string commandName, string commandHelp, Func<InvokeParameters, Task<int>> invoker) => new (commandName, commandHelp, invoker);

    public CommandConfiguration WithOrderId(int orderId)
    {
        OrderId = orderId;
        return this;
    }

    public CommandConfiguration WithRequiredArgument(string commandName, string description)
    {
        RequiredArguments.Add(new RequiredArgument(commandName, description));
        return this;
    }

    public OptionsBuilder BuildCommand() => OptionsBuilder;

    public record RequiredArgument(string CommandName, string Description);
    public record InvokeParameters(IImmutableList<CommandConfiguration> ConfiguredCommands, IDictionary<string, string> RequiredParameters, Action<string> MessagePump)
    {
        public T RequiredParameterToValue<T>(string key) => (T)Convert.ChangeType(RequiredParameters[key], typeof(T));
    }
}
