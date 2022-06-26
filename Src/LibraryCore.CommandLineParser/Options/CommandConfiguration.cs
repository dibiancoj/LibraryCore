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

    public record RequiredArgument(string CommandName, string Description);
    public record OptionalArgument(string Flag, string Description, bool ArgumentRequiresParameterAfterFlag);

    public CommandConfiguration(string commandName, string commandHelp, Func<InvokeParameters, Task<int>> invoker, OptionsBuilder optionsBuilder)
    {
        RequiredArguments = ImmutableList<RequiredArgument>.Empty;
        OptionalArguments = ImmutableList<OptionalArgument>.Empty;
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
    public IImmutableList<OptionalArgument> OptionalArguments { get; private set; }

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

    public CommandConfiguration WithOptionalArgument(string flag, string description, bool argumentRequiresParameterAfterFlag)
    {
        //create another list because we want this to be immutable when we share it out.
        OptionalArguments = OptionalArguments.Add(new OptionalArgument(flag, description, argumentRequiresParameterAfterFlag));
        return this;
    }

    public OptionsBuilder BuildCommand() => OptionsBuilder;

}
