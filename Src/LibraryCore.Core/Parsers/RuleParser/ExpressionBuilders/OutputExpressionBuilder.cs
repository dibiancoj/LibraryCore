using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.ExpressionBuilders;

public static class OutputExpressionBuilder
{
    public static Expression<Func<TResult>> BuildExpression<TResult>(IEnumerable<IToken> tokens)
    {
        var parametersToUse = Array.Empty<ParameterExpression>();

        return Expression.Lambda<Func<TResult>>(tokens.Single().CreateExpression(parametersToUse), parametersToUse);
    }

    public static Expression<Func<T1, TResult>> BuildExpression<T1, TResult>(IEnumerable<IToken> tokens, string parameterOneName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parametersToUse = new[] { parameter1 };

        var expressionToExecute = tokens.Single().CreateExpression(parametersToUse);

        return Expression.Lambda<Func<T1, TResult>>(expressionToExecute, parameter1);
    }

    public static Expression<Func<T1, T2, TResult>> BuildExpression<T1, T2, TResult>(IEnumerable<IToken> tokens, string parameterOneName, string parameterTwoName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parameter2 = Expression.Parameter(typeof(T2), parameterTwoName);
        var parametersToUse = new[] { parameter1, parameter2 };

        var expressionToExecute = tokens.Single().CreateExpression(parametersToUse);

        return Expression.Lambda<Func<T1, T2, TResult>>(expressionToExecute, parameter1, parameter2);
    }
}

