using LibraryCore.Core.DataTypes;
using LibraryCore.Parsers.RuleParser.ExpressionBuilders;
using LibraryCore.Parsers.RuleParser.TokenFactories;
using System.Collections.Immutable;
using System.Linq.Expressions;
using static LibraryCore.Parsers.RuleParser.TokenFactories.Implementation.ScoreToken;

namespace LibraryCore.Parsers.RuleParser;

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
    public Expression<Func<bool>> BuildExpression()
    {
        var parametersToUse = ImmutableList<ParameterExpression>.Empty;

        return Expression.Lambda<Func<bool>>(RuleParserExpressionBuilder.CreateExpression(CompilationTokenResult, parametersToUse), parametersToUse);
    }

    public Expression<Func<T1, bool>> BuildExpression<T1>(string parameter1Name)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameter1Name);
        var parametersToUse = new[] { parameter1 }.ToImmutableList();

        return Expression.Lambda<Func<T1, bool>>(RuleParserExpressionBuilder.CreateExpression(CompilationTokenResult, parametersToUse), parametersToUse);
    }

    public Expression<Func<T1, T2, bool>> BuildExpression<T1, T2>(string parameter1Name, string parameter2Name)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameter1Name);
        var parameter2 = Expression.Parameter(typeof(T2), parameter2Name);
        var parametersToUse = new[] { parameter1, parameter2 }.ToImmutableList();

        return Expression.Lambda<Func<T1, T2, bool>>(RuleParserExpressionBuilder.CreateExpression(CompilationTokenResult, parametersToUse), parametersToUse);
    }

    public Expression<Func<T1, T2, T3, bool>> BuildExpression<T1, T2, T3>(string parameter1Name, string parameter2Name, string parameter3Name)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameter1Name);
        var parameter2 = Expression.Parameter(typeof(T2), parameter2Name);
        var parameter3 = Expression.Parameter(typeof(T3), parameter3Name);
        var parametersToUse = new[] { parameter1, parameter2, parameter3 }.ToImmutableList();

        return Expression.Lambda<Func<T1, T2, T3, bool>>(RuleParserExpressionBuilder.CreateExpression(CompilationTokenResult, parametersToUse), parametersToUse);
    }

    /// <summary>
    /// Try to cache the compiled expression if this is used for a log 
    /// </summary>
    public Expression<Func<string>> BuildStringExpression()
    {
        var parametersToUse = ImmutableList<ParameterExpression>.Empty;

        return Expression.Lambda<Func<string>>(CompilationTokenResult.Single().CreateExpression(parametersToUse), parametersToUse);
    }

    /// <summary>
    /// Try to cache the compiled expression if this is used for a log 
    /// </summary>
    public Expression<Func<T1, string>> BuildStringExpression<T1>(string parameter1Name)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameter1Name);
        var parametersToUse = new[] { parameter1 }.ToImmutableList();

        var expressionToExecute = CompilationTokenResult.Single().CreateExpression(parametersToUse);

        return Expression.Lambda<Func<T1, string>>(expressionToExecute, parameter1);
    }

    /// <summary>
    /// Try to cache the compiled expression if this is used for a log 
    /// </summary>
    public Expression<Func<T1, T2, string>> BuildStringExpression<T1, T2>(string parameter1Name, string parameter2Name)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameter1Name);
        var parameter2 = Expression.Parameter(typeof(T2), parameter2Name);
        var parametersToUse = new[] { parameter1, parameter2 }.ToImmutableList();

        var expressionToExecute = CompilationTokenResult.Single().CreateExpression(parametersToUse);

        return Expression.Lambda<Func<T1, T2, string>>(expressionToExecute, parameter1, parameter2);
    }
}

public class RuleParserCompilationResult<TResult>
{
    public RuleParserCompilationResult(IImmutableList<IToken> compilationTokeResult)
    {
        CompilationTokenResult = compilationTokeResult;
    }

    public IImmutableList<IToken> CompilationTokenResult { get; }

    public Expression<Func<T1, TResult>> BuildScoreExpression<T1>(ScoringMode scoringMode, string parameter1Name)
    {
        if (!IsValidScoringMode(scoringMode))
        {
            throw new ArgumentOutOfRangeException(nameof(scoringMode), "AccumulatedScore Is Only Valid For Number Like Scoring Values (int16, int, int64, decimal)");
        }

        var parameter1 = Expression.Parameter(typeof(T1), parameter1Name);
        var parametersToUse = new[] { parameter1 }.ToImmutableList();

        return Expression.Lambda<Func<T1, TResult>>(RuleParserExpressionBuilder.CreateRuleExpression<TResult>(scoringMode, CompilationTokenResult, parametersToUse), parametersToUse);
    }

    public Expression<Func<T1, T2, TResult>> BuildScoreExpression<T1, T2>(ScoringMode scoringMode, string parameter1Name, string parameter2Name)
    {
        if (!IsValidScoringMode(scoringMode))
        {
            throw new ArgumentOutOfRangeException(nameof(scoringMode), "AccumulatedScore Is Only Valid For Number Like Scoring Values (int16, int, int64, decimal)");
        }

        var parameter1 = Expression.Parameter(typeof(T1), parameter1Name);
        var parameter2 = Expression.Parameter(typeof(T2), parameter2Name);
        var parametersToUse = new[] { parameter1, parameter2 }.ToImmutableList();

        return Expression.Lambda<Func<T1, T2, TResult>>(RuleParserExpressionBuilder.CreateRuleExpression<TResult>(scoringMode, CompilationTokenResult, parametersToUse), parametersToUse);
    }

    private static bool IsValidScoringMode(ScoringMode scoringMode) => scoringMode == ScoringMode.ShortCircuitOnFirstTrueEval || PrimitiveTypes.NumberTypesSelect().Contains(typeof(TResult));
}