using System.Text;

namespace LibraryCore.CommandLineParser.Options;

public class OptionBuilder
{
    //MyCli [CommandName] [RequiredArgument] -p {optionalArgument}
    //MyCli [CommandName] [RequiredArgument] -v

    //Types of parameters
    //Command Name
    //Required Args
    //Optional Args With No Parameter
    //Optional Args With Parameters

    public OptionBuilder()
    {
        Commands = new List<CommandBuilder>();
    }

    private List<CommandBuilder> Commands { get; }

    public CommandBuilder AddCommand(Func<int> invoker, string commandName, string commandHelp)
    {
        var command = new CommandBuilder(this, invoker, commandName, commandHelp);
        Commands.Add(command);
        return command;
    }

    public int Run(string[] commandArgs)
    {
        var baseCommand = commandArgs[0];

        if (baseCommand == "?")
        {
            Console.WriteLine(HelpMenu());
            return 0;
        }

        var commandToRun = Commands.SingleOrDefault(x => baseCommand.Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));

        if (commandToRun == null)
        {
            Console.WriteLine("No Command Found For {0}", baseCommand);
            Console.WriteLine(HelpMenu());

            return 1;
        }

        return commandToRun.Invoker();
    }

    public string HelpMenu()
    {
        return new StringBuilder()
            .AppendLine("Help Menu")
            .ToString();
    }

}
