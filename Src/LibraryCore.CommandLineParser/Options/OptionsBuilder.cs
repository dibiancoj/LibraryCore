using LibraryCore.CommandLineParser.DefaultCommands;
using LibraryCore.CommandLineParser.RunnerModels;
using System.Collections.Immutable;

namespace LibraryCore.CommandLineParser.Options;

public class OptionsBuilder
{
    //MyCli [CommandName] [RequiredArgument] -p {optionalArgument}
    //MyCli [CommandName] [RequiredArgument] -v

    //Types of parameters
    //Command Name
    //Required Args
    //Optional Args With No Parameter
    //Optional Args With Parameters

    public OptionsBuilder()
    {
        Commands = new[] { HelpCommand.AddHelpCommand(this) }.ToImmutableList();
    }

    internal IImmutableList<CommandConfiguration> Commands { get; set; }

    public CommandConfiguration AddCommand(string commandName, string commandHelp, Func<InvokeParameters, Task<int>> invoker)
    {
        var command = new CommandConfiguration(commandName, commandHelp, invoker, this);
        Commands = Commands.Add(command);
        return command;
    }

}
