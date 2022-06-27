using LibraryCore.CommandLineParser.DefaultCommands;
using LibraryCore.CommandLineParser.Options;
using System.Collections.Immutable;

namespace LibraryCore.CommandLineParser;

public static class Runner
{
    public static async Task<int> RunAsync(string[] commandArgs, OptionsBuilder configuration)
    {
        const string helpCommand = "?";

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
            commandToRun = configuration.Commands.Single(x => helpCommand.Equals(x.CommandName, StringComparison.OrdinalIgnoreCase));
        }

        verboseModeWriter($"Command To Invoke = {commandToRun.CommandName}");

        //sub command help
        if (FindIndex(commandArgs.Skip(1), x => x == helpCommand) > -1)
        {
            Console.WriteLine(HelpCommand.HelpMenuTextForSubCommand(commandToRun));
            return 0;
        }

        //need to skip the first argument which is the base command
        var argumentsSpecifiedAtRunTimeByUser = commandArgs.Skip(1).ToImmutableList();

        //Environment.Exit(code);
        return await commandToRun.Invoker(new InvokeParameters
        {
            ConfiguredCommands = configuration.Commands.ToImmutableList(),
            RequiredArguments = ParseToRequiredArguments(argumentsSpecifiedAtRunTimeByUser, commandToRun, verboseModeWriter).ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
            OptionalArguments = ParseOptionalArguments(argumentsSpecifiedAtRunTimeByUser, commandToRun, verboseModeWriter).ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
            MessagePump = verboseModeWriter
        });
    }

    private static IDictionary<string, string?> ParseOptionalArguments(IImmutableList<string> commandArgs, CommandConfiguration commandToRun, Action<string> verboseModeWriter)
    {
        if (commandToRun.OptionalArguments.Count == 0)
        {
            return ImmutableDictionary<string, string?>.Empty;
        }

        var returnValue = new Dictionary<string, string?>();

        foreach (var optionalArgRegistered in commandToRun.OptionalArguments)
        {
            var indexOfCommand = FindIndex(commandArgs, x => string.Equals(x, optionalArgRegistered.Flag, StringComparison.OrdinalIgnoreCase));

            if (indexOfCommand > -1)
            {
                if (optionalArgRegistered.ArgumentRequiresParameterAfterFlag)
                {
                    //if has command after check if the we can grab it with index +=1 ...
                    if (indexOfCommand + 1 >= commandArgs.Count)
                    {
                        throw new Exception($"Optional Argument Name = {optionalArgRegistered.Flag} | Has Missing Optional Arguments");
                    }
                    else
                    {
                        returnValue.Add(optionalArgRegistered.Flag, commandArgs[indexOfCommand + 1]);
                    }
                }
                else
                {
                    //no command after this...just say its here
                    returnValue.Add(optionalArgRegistered.Flag, null);
                }
            }
        }

        return returnValue;
    }

    private static IDictionary<string, string> ParseToRequiredArguments(IImmutableList<string> commandArgs, CommandConfiguration commandToRun, Action<string> verboseModeWriter)
    {
        if (commandToRun.RequiredArguments.Count > 0 && commandArgs.Count < commandToRun.RequiredArguments.Count)
        {
            throw new Exception("Missing Required Arguments");
        }

        var temp = commandToRun.RequiredArguments.Select((command, i) => new
        {
            Command = command,
            Value = commandArgs[i]
        });

        verboseModeWriter(string.Join(Environment.NewLine, temp.Select(t => $"Required Parameter Name = {t.Command.CommandName} | Value = {t.Value}")));

        return temp.ToDictionary(x => x.Command.CommandName, x => x.Value);
    }

    private static int FindIndex<T>(IEnumerable<T> listOfT, Func<T, bool> predicate)
    {
        int i = 0;

        foreach (var item in listOfT)
        {
            if (predicate(item))
            {
                return i;
            }

            i++;
        }

        return -1;
    }
}
