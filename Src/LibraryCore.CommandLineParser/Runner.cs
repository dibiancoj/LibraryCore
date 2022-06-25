using LibraryCore.CommandLineParser.Options;
using System.Collections.Immutable;
using static LibraryCore.CommandLineParser.Options.CommandConfiguration;

namespace LibraryCore.CommandLineParser;

public static class Runner
{
    public static async Task<int> RunAsync(string[] commandArgs, OptionsBuilder configuration)
    {
        var baseCommand = commandArgs.ElementAtOrDefault(0);

        if (string.IsNullOrEmpty(baseCommand))
        {
            Console.WriteLine("No Command Specified In Input Args");
            return 1;
        }

        bool isVerboseMode = commandArgs.Any(x => x == "-v" || x == "-V");
        Action<string> verboseModeWriter = isVerboseMode ?
                                                message => Console.WriteLine(message) :
                                                message => { };

        var commandToRun = configuration.Commands.SingleOrDefault(x => baseCommand.Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));

        if (commandToRun == null)
        {
            Console.WriteLine("No Command Found For {0}", baseCommand);
            commandToRun = configuration.Commands.Single(x => "?".Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));
        }

        //Environment.Exit(code);
        return await commandToRun.Invoker(new InvokeParameters(configuration.Commands.ToImmutableList(), ParseToRequiredArguments(commandArgs, commandToRun, verboseModeWriter), verboseModeWriter));
    }

    private static IDictionary<string, string> ParseToRequiredArguments(string[] commandArgs, CommandConfiguration commandToRun, Action<string> verboseModeWriter)
    {
        var parameters = new Dictionary<string, string>();
        var commandArgsAfterInitialCommand = commandArgs.Skip(1).ToArray();

        verboseModeWriter($"Command To Invoke = {commandToRun.CommandName}");

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
            var value = commandArgsAfterInitialCommand[i];

            verboseModeWriter($"Required Parameter Name = {requiredParameter.CommandName} | Value = {value}");
            parameters.Add(requiredParameter.CommandName, value);
            i++;
        }

        return parameters;
    }
}
