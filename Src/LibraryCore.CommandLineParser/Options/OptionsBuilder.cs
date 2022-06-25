using LibraryCore.CommandLineParser.DefaultCommands;
using static LibraryCore.CommandLineParser.Options.CommandConfiguration;
using static LibraryCore.CommandLineParser.Runner;

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
        Commands = new List<CommandConfiguration>
        {
            HelpCommand.AddHelpCommand(this)
        };
    }

    internal List<CommandConfiguration> Commands { get; set; }

    public CommandConfiguration AddCommand(string commandName, string commandHelp, Func<InvokeParameters, Task<int>> invoker)
    { 
        var command = new CommandConfiguration(commandName, commandHelp, invoker, this);
        Commands.Add(command);
        return command;
    }

}
