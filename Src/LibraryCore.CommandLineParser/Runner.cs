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
            Console.WriteLine("No Command Registered For {0}", baseCommand);
            commandToRun = configuration.Commands.Single(x => "?".Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));
        }

        //Environment.Exit(code);
        return await commandToRun.Invoker(new InvokeParameters
        {
            ConfiguredCommands = configuration.Commands.ToImmutableList(),
            RequiredArguments = ParseToRequiredArguments(commandArgs, commandToRun, verboseModeWriter).ToImmutableDictionary(),
            MessagePump = verboseModeWriter
        });
    }

    private static IDictionary<string, string> ParseToRequiredArguments(string[] commandArgs, CommandConfiguration commandToRun, Action<string> verboseModeWriter)
    {
        var commandArgsAfterInitialCommand = commandArgs.Skip(1).ToArray();

        verboseModeWriter($"Command To Invoke = {commandToRun.CommandName}");

        if (commandToRun.RequiredArguments.Count > 0 && commandArgsAfterInitialCommand.Length < commandToRun.RequiredArguments.Count)
        {
            throw new Exception("Missing Required Arguments");
        }

        var temp = commandToRun.RequiredArguments.Select((command, i) => new
        {
            Command = command,
            Value = commandArgsAfterInitialCommand[i]
        });

        verboseModeWriter(string.Join(Environment.NewLine, temp.Select(t => $"Required Parameter Name = {t.Command.CommandName} | Value = {t.Value}")));

        return temp.ToDictionary(x => x.Command.CommandName, x => x.Value);
    }
}
