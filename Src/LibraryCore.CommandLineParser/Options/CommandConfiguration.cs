using LibraryCore.CommandLineParser.RunnerModels;
using System.Collections.Immutable;
using System.Diagnostics;

namespace LibraryCore.CommandLineParser.Options;

[DebuggerDisplay("Command = {CommandName}")]
public class CommandConfiguration(string commandName, string commandHelp, Func<InvokeParameters, Task<int>> invoker, OptionsBuilder optionsBuilder)
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

    public Func<InvokeParameters, Task<int>> Invoker { get; } = invoker;
    private OptionsBuilder OptionsBuilder { get; } = optionsBuilder;
    public string CommandName { get; } = commandName;
    public string CommandHelp { get; } = commandHelp;
    public int? OrderId { get; private set; }
    public IImmutableList<RequiredArgument> RequiredArguments { get; private set; } = ImmutableList<RequiredArgument>.Empty;
    public IImmutableList<OptionalArgument> OptionalArguments { get; private set; } = ImmutableList<OptionalArgument>.Empty;

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
