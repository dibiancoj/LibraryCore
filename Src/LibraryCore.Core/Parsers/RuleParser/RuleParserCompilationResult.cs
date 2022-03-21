using LibraryCore.Core.Parsers.RuleParser.ExpressionBuilders;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser;

public class RuleParserCompilationResult
{
    public RuleParserCompilationResult(IImmutableList<IToken> compilationTokeResult)
    {
        CompilationTokenResult = compilationTokeResult;
    }

    public IImmutableList<IToken> CompilationTokenResult { get; }

    /// <summary>
    /// facade the build expressions so we don't need to declare variables. Keep the expression builder seperate but make it easier for the caller
    /// </summary>

    public Expression<Func<bool>> BuildExpression() => RuleParserExpressionBuilder.BuildExpression(CompilationTokenResult);

    public Expression<Func<T1, bool>> BuildExpression<T1>(string parameter1Name) => RuleParserExpressionBuilder.BuildExpression<T1>(CompilationTokenResult, parameter1Name);
    public Expression<Func<T1, T2, bool>> BuildExpression<T1, T2>(string parameter1Name, string parameter2Name) =>
                    RuleParserExpressionBuilder.BuildExpression<T1, T2>(CompilationTokenResult, parameter1Name, parameter2Name);

    public Expression<Func<T1, T2, T3, bool>> BuildExpression<T1, T2, T3>(string parameter1Name, string parameter2Name, string parameter3Name) =>
                    RuleParserExpressionBuilder.BuildExpression<T1, T2, T3>(CompilationTokenResult, parameter1Name, parameter2Name, parameter3Name);


    public Expression<Func<string>> BuildStringExpression() => StringOutputExpressionBuilder.BuildExpression(CompilationTokenResult);

    public Expression<Func<T1, string>> BuildStringExpression<T1>(string parameter1Name) => StringOutputExpressionBuilder.BuildExpression<T1>(CompilationTokenResult, parameter1Name);

}
