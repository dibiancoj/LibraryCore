using LibraryCore.CommandLineParser.Options;
using System.Text;
using static LibraryCore.CommandLineParser.Options.CommandBuilder;

namespace LibraryCore.CommandLineParser.DefaultCommands;

public class HelpCommand
{
    internal static void AddHelpCommand(OptionBuilder optionBuilder, List<CommandBuilder> commands)
    {
        optionBuilder.AddCommand((InvokeParameters parameters) => HelpMenu(commands), "?", "Help Menu", 0);
    }

    private static int HelpMenu(IEnumerable<CommandBuilder> commands)
    {
        Console.WriteLine(HelpMenuText(commands));
        return 0;
    }

    private static string HelpMenuText(IEnumerable<CommandBuilder> commands)
    {
        var builder = new StringBuilder("Help Menu");

        builder.AppendLine(Environment.NewLine);
        builder.AppendLine("--- Commands ---");

        foreach (var command in commands)
        {
            builder.AppendLine($"{command.CommandName} - {command.CommandHelp}");
        }

        return builder.ToString();
    }
}
