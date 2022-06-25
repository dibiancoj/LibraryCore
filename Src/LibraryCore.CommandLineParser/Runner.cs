using LibraryCore.CommandLineParser.Options;
using System.Collections.Immutable;

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

        Action<string> verboseModeWriter = commandArgs.Any(x => x == "-v" || x == "-V") ?
                                                message => Console.WriteLine(message) :
                                                message => { };

        var commandToRun = configuration.Commands.SingleOrDefault(x => baseCommand.Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));

        if (commandToRun == null)
        {
            Console.WriteLine("No Command Found For {0}", baseCommand);
            commandToRun = configuration.Commands.Single(x => "?".Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));
        }

        //Environment.Exit(code);
        return await commandToRun.Invoker(new InvokeParameters
        {
            ConfiguredCommands = configuration.Commands.ToImmutableList(),
            RequiredParameters = ParseToRequiredArguments(commandArgs, commandToRun, verboseModeWriter),
            MessagePump = verboseModeWriter
        });
    }

    private static IDictionary<string, string> ParseToRequiredArguments(string[] commandArgs, CommandConfiguration commandToRun, Action<string> verboseModeWriter)
    {
        var parameters = new Dictionary<string, string>();
        var commandArgsAfterInitialCommand = commandArgs.Skip(1).ToArray();

        verboseModeWriter($"Command To Invoke = {commandToRun.CommandName}");

        if (commandToRun.RequiredArguments.Count > 0 && commandArgsAfterInitialCommand.Length < commandToRun.RequiredArguments.Count)
        {
            throw new Exception("Missing Required Arguments");
        }

        for (int i = 0; i < commandToRun.RequiredArguments.Count; i++)
        {
            var commandArgFoundInBootup = commandArgsAfterInitialCommand[i];
            var requiredParameter = commandToRun.RequiredArguments[i];

            verboseModeWriter($"Required Parameter Name = {requiredParameter.CommandName} | Value = {commandArgFoundInBootup}");

            parameters.Add(requiredParameter.CommandName, commandArgFoundInBootup);
        }

        return parameters;
    }
}
