using LibraryCore.CommandLineParser.DefaultCommands;

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
            HelpCommand.AddHelpCommand()
        };
    }

    internal List<CommandConfiguration> Commands { get; set; }

    public OptionsBuilder AddCommand(CommandConfiguration commandConfiguration)
    {
        Commands.Add(commandConfiguration);
        return this;
    }

}
