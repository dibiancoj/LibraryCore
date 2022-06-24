using LibraryCore.CommandLineParser.DefaultCommands;
using static LibraryCore.CommandLineParser.Options.CommandBuilder;

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

    private List<CommandBuilder> Commands { get; set; }

    public CommandBuilder AddCommand(Func<InvokeParameters, int> invoker, string commandName, string commandHelp, int? index = null)
    {
        var command = new CommandBuilder(this, invoker, commandName, commandHelp);

        if (index.HasValue)
        {
            Commands.Insert(index.Value, command);
        }
        else
        {
            Commands.Add(command);
        }

        return command;
    }

    public int Run(string[] commandArgs)
    {
        HelpCommand.AddHelpCommand(this, Commands);

        var baseCommand = commandArgs.ElementAtOrDefault(0);

        if (string.IsNullOrEmpty(baseCommand))
        {
            Console.WriteLine("No Command Specified In Input Args");
            return 1;
        }

        var commandToRun = Commands.SingleOrDefault(x => baseCommand.Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));

        if (commandToRun == null)
        {
            Console.WriteLine("No Command Found For {0}", baseCommand);
            var helpCommand = Commands.Single(x => "?".Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));
            helpCommand.Invoker(new InvokeParameters(ParseToRequiredArguments(commandArgs, helpCommand)));

            return 1;
        }

        return commandToRun.Invoker(new InvokeParameters(ParseToRequiredArguments(commandArgs, commandToRun)));
    }

    private static IDictionary<string, string> ParseToRequiredArguments(string[] commandArgs, CommandBuilder commandToRun)
    {
        var parameters = new Dictionary<string, string>();
        var commandArgsAfterInitialCommand = commandArgs.Skip(1).ToArray();

        //always be 1 because the first is the command
        if (commandArgsAfterInitialCommand.Length == 0)
        {
            return parameters;
        }

        if (commandArgsAfterInitialCommand.Length < commandToRun.RequiredArguments.Count)
        {
            throw new Exception("Missing Required Arguments");
        }

        int i = 0;

        foreach (var requiredParameter in commandToRun.RequiredArguments)
        {
            parameters.Add(requiredParameter.CommandName, commandArgsAfterInitialCommand[i]);
            i++;
        }

        return parameters;
    }

}
