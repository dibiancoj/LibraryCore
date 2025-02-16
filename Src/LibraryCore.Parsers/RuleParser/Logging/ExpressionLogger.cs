namespace LibraryCore.Parsers.RuleParser.Logging;

public record LogResult(string Message, bool Result);
public record ParameterResult(string Name, string Value);

public interface IExpressionLogger
{
    public void AddLogRecord(string message, bool result);
    public void AddParameter(string parameterName, string parameterValue);
    public IEnumerable<LogResult> LogRecords();
    public IEnumerable<ParameterResult> ParameterRecords();
}

public class ExpressionLogger : IExpressionLogger
{
    private List<LogResult> Records { get; } = [];
    private Lazy<List<ParameterResult>> Parameters { get; } = new();

    public void AddLogRecord(string message, bool result) => Records.Add(new LogResult(message, result));
    public void AddParameter(string parameterName, string parameterValue) => Parameters.Value.Add(new ParameterResult(parameterName, parameterValue));

    public IEnumerable<LogResult> LogRecords() => Records.AsEnumerable();
    public IEnumerable<ParameterResult> ParameterRecords() => Parameters.Value;
}