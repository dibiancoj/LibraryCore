using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.ExpressionBuilders;

public static class StringOutputExpressionBuilder
{
    public static Expression<Func<string>> BuildExpression(IEnumerable<IToken> tokens)
    {
        var parametersToUse = Array.Empty<ParameterExpression>();

        return Expression.Lambda<Func<string>>(tokens.Single().CreateExpression(parametersToUse), parametersToUse);
    }

    public static Expression<Func<T1, string>> BuildExpression<T1>(IEnumerable<IToken> tokens, string parameterOneName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parametersToUse = new[] { parameter1 };

        var expressionToExecute = tokens.Single().CreateExpression(parametersToUse);

        return Expression.Lambda<Func<T1, string>>(expressionToExecute, parameter1);
    }

    public static Expression<Func<T1, T2, string>> BuildExpression<T1, T2>(IEnumerable<IToken> tokens, string parameterOneName, string parameterTwoName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parameter2 = Expression.Parameter(typeof(T2), parameterTwoName);
        var parametersToUse = new[] { parameter1, parameter2 };

        var expressionToExecute = tokens.Single().CreateExpression(parametersToUse);

        return Expression.Lambda<Func<T1, T2, string>>(expressionToExecute, parameter1, parameter2);
    }
}

