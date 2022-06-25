using System.Collections.Immutable;
using System.Diagnostics;
using static LibraryCore.CommandLineParser.Runner;

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

    public record RequiredArgument(string CommandName, string Description);

    public CommandConfiguration(string commandName, string commandHelp, Func<InvokeParameters, Task<int>> invoker, OptionsBuilder optionsBuilder)
    {
        RequiredArguments = ImmutableList<RequiredArgument>.Empty;
        Invoker = invoker;
        OptionsBuilder = optionsBuilder;
        CommandName = commandName;
        CommandHelp = commandHelp;
    }

    public Func<InvokeParameters, Task<int>> Invoker { get; }
    private OptionsBuilder OptionsBuilder { get; }
    public string CommandName { get; }
    public string CommandHelp { get; }
    public int? OrderId { get; private set; }
    public IImmutableList<RequiredArgument> RequiredArguments { get; private set; }

    public CommandConfiguration WithOrderId(int orderId)
    {
        OrderId = orderId;
        return this;
    }

    public CommandConfiguration WithRequiredArgument(string commandName, string description)
    {
        //create another list because we want this to be immutable when we share it out.
        RequiredArguments = RequiredArguments.Add(new RequiredArgument(commandName, description));
        return this;
    }

    public OptionsBuilder BuildCommand() => OptionsBuilder;

}
