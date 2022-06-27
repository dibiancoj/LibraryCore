using LibraryCore.CommandLineParser.Options;
using System.Text;

namespace LibraryCore.CommandLineParser.DefaultCommands;

public static class HelpCommand
{
    internal static CommandConfiguration AddHelpCommand(OptionsBuilder optionsBuilder)
    {
        return new CommandConfiguration("?", "Help Menu", parameters => HelpMenu(parameters.ConfiguredCommands), optionsBuilder)
                        .WithOrderId(1);
    }

    private static Task<int> HelpMenu(IEnumerable<CommandConfiguration> commands)
    {
        Console.WriteLine(HelpMenuText(commands));
        return Task.FromResult(0);
    }

    private static string HelpMenuText(IEnumerable<CommandConfiguration> commands)
    {
        var builder = new StringBuilder("Help Menu");

        builder.AppendLine(Environment.NewLine);
        builder.AppendLine("--- Commands ---");

        foreach (var command in commands.OrderBy(x => x.OrderId ?? int.MaxValue).ThenBy(x => x.CommandName))
        {
            builder.AppendLine($"{command.CommandName} - {command.CommandHelp}");
        }

        builder.AppendLine("-v - verbose");

        return builder.ToString();
    }

    internal static string HelpMenuTextForSubCommand(CommandConfiguration command)
    {
        var builder = new StringBuilder("Help Menu");

        builder.AppendLine(Environment.NewLine);
        builder.AppendLine("--- Command ---");
        builder.AppendLine(command.CommandName);

        foreach (var subCommand in command.OptionalArguments.OrderBy(x => x.Flag))
        {
            builder.AppendLine($"{subCommand.Flag} - {subCommand.Description}");
        }

        builder.AppendLine("-v - verbose");

        return builder.ToString();
    }
}
