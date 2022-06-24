namespace LibraryCore.CommandLineParser.Options;

public class CommandBuilder
{
    //MyCli [CommandName] [RequiredArgument] -p {optionalArgument}
    //MyCli [CommandName] [RequiredArgument] -v

    //Types of parameters
    //Command Name
    //Required Args
    //Optional Args With No Parameter
    //Optional Args With Parameters

    public CommandBuilder(OptionBuilder configurationOptions, Func<int> invoker, string commandName, string commandHelp)
    {
        RequiredArguments = new List<CommandModel>();
        Invoker = invoker;
        Option = configurationOptions;
        CommandName = commandName;
        CommandHelp = commandHelp;
    }

    public Func<int> Invoker { get; }
    private OptionBuilder Option { get; }
    public string CommandName { get; }
    public string CommandHelp { get; }
    public List<CommandModel> RequiredArguments { get; }

    public CommandBuilder AddRequiredArgument(string commandName, string description)
    {
        RequiredArguments.Add(new CommandModel(commandName, description));
        return this;
    }

    //public CommandBuilder AddRequiredArgument(string commandName, string description)
    //{
    //    RequiredArguments.Add((commandName, description));
    //    return this;
    //}

    public OptionBuilder Create() => Option;

    public record CommandModel(string CommandName, string Description);
}
