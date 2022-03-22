using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.ExpressionBuilders;

public static class RuleParserExpressionBuilder
{
    public static Expression<Func<bool>> BuildExpression(IEnumerable<IToken> tokens)
    {
        var parametersToUse = Array.Empty<ParameterExpression>();

        return Expression.Lambda<Func<bool>>(CreateExpression(tokens, parametersToUse), parametersToUse);
    }

    public static Expression<Func<T1, bool>> BuildExpression<T1>(IEnumerable<IToken> tokens, string parameterOneName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parametersToUse = new[] { parameter1 };

        return Expression.Lambda<Func<T1, bool>>(CreateExpression(tokens, parametersToUse), parametersToUse);
    }

    public static Expression<Func<T1, T2, bool>> BuildExpression<T1, T2>(IEnumerable<IToken> tokens, string parameterOneName, string parameterTwoName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parameter2 = Expression.Parameter(typeof(T2), parameterTwoName);
        var parametersToUse = new[] { parameter1, parameter2 };

        return Expression.Lambda<Func<T1, T2, bool>>(CreateExpression(tokens, parametersToUse), parametersToUse);
    }

    public static Expression<Func<T1, T2, T3, bool>> BuildExpression<T1, T2, T3>(IEnumerable<IToken> tokens, string parameterOneName, string parameterTwoName, string parameterThreeName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parameter2 = Expression.Parameter(typeof(T2), parameterTwoName);
        var parameter3 = Expression.Parameter(typeof(T3), parameterThreeName);
        var parametersToUse = new[] { parameter1, parameter2, parameter3 };

        return Expression.Lambda<Func<T1, T2, T3, bool>>(CreateExpression(tokens, parametersToUse), parametersToUse);
    }

    private static Expression CreateExpression(IEnumerable<IToken> tokens, ParameterExpression[] parametersToUse)
    {
        Expression? workingExpression = null;

        Expression? left = null;
        Expression? right = null;
        IBinaryComparisonToken? operation = null;
        IBinaryExpressionCombiner? combiner = null;

        foreach (var token in tokens.Where(x => x is not WhiteSpaceToken))
        {
            Expression temp = null!;

            if (token is IBinaryComparisonToken tempBinaryComparisonToken)
            {
                //==, !=, >, >=, <=
                operation = tempBinaryComparisonToken;
            }
            else if (token is IBinaryExpressionCombiner tempBinaryExpressionCombiner)
            {
                //AndAlso OrElse
                combiner = tempBinaryExpressionCombiner;
            }
            else
            {
                //normal clause
                temp = token.CreateExpression(parametersToUse);
            }

            if (left == null)
            {
                left = temp;
            }
            else if (right == null)
            {
                right = temp;
            }

            if (left != null && right != null && operation != null)
            {
                //at this point we have an operator because we have a left and a right
                var currentWorkingExpression = operation.CreateBinaryOperatorExpression(left, right);

                //combiner would be && , ||
                workingExpression = combiner == null ?
                        currentWorkingExpression :
                        combiner.CreateBinaryOperatorExpression(workingExpression ?? throw new NullReferenceException($"{nameof(workingExpression)} Is Null"),
                                                                currentWorkingExpression ?? throw new NullReferenceException($"{nameof(currentWorkingExpression)} is null"));

                left = null;
                operation = null;
                right = null;
                combiner = null;
            }
        }

        return workingExpression ?? throw new Exception("No Expressions Found");
    }
}
