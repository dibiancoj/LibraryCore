namespace LibraryCore.CommandLineParser.RunnerModels;

internal class CommandLineParserException : Exception
{
    public CommandLineParserException(string? message) : base(message)
    {
    }
}
