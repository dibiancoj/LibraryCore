namespace LibraryCore.Parsers.RuleParser.Logging;

public record LogResult(string Message, bool Result);

public class ExpressionLogger
{
    private List<LogResult> Records { get; } = new();

    public void Add(LogResult logResult) => Records.Add(logResult);

    public IEnumerable<LogResult> LogRecords() => Records.AsEnumerable();
}
