using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser;

public static class RuleParserExpressionBuilder
{
    public static Expression<Func<bool>> BuildExpression<T1>(IEnumerable<Token> tokens)
    {
        var parametersToUse = Array.Empty<ParameterExpression>();

        return Expression.Lambda<Func<bool>>(CreateExpression(tokens, parametersToUse), parametersToUse);
    }

    public static Expression<Func<T1, bool>> BuildExpression<T1>(IEnumerable<Token> tokens, string parameterOneName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parametersToUse = new[] { parameter1 };

        return Expression.Lambda<Func<T1, bool>>(CreateExpression(tokens, parametersToUse), parametersToUse);
    }

    public static Expression<Func<T1, T2, bool>> BuildExpression<T1, T2>(IEnumerable<Token> tokens, string parameterOneName, string parameterTwoName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parameter2 = Expression.Parameter(typeof(T2), parameterTwoName);
        var parametersToUse = new[] { parameter1, parameter2 };

        return Expression.Lambda<Func<T1, T2, bool>>(CreateExpression(tokens, parametersToUse), parametersToUse);
    }

    public static Expression<Func<T1, T2, T3, bool>> BuildExpression<T1, T2, T3>(IEnumerable<Token> tokens, string parameterOneName, string parameterTwoName, string parameterThreeName)
    {
        var parameter1 = Expression.Parameter(typeof(T1), parameterOneName);
        var parameter2 = Expression.Parameter(typeof(T2), parameterTwoName);
        var parameter3 = Expression.Parameter(typeof(T3), parameterThreeName);
        var parametersToUse = new[] { parameter1, parameter2, parameter3 };

        return Expression.Lambda<Func<T1, T2, T3, bool>>(CreateExpression(tokens, parametersToUse), parametersToUse);
    }

    private static Expression CreateExpression(IEnumerable<Token> tokens, ParameterExpression[] parametersToUse)
    {
        Expression? workingExpression = null;

        Expression? left = null;
        Expression? right = null;
        Token? operation = null;
        Token? combiner = null;

        foreach (var token in tokens.Where(x => x is not WhiteSpaceToken))
        {
            Expression temp = null!;

            if (token is IBinaryComparisonToken)
            {
                //==, >, >=, <=
                operation = token;
            }
            else if (token is IBinaryExpressionCombiner)
            {
                //AndAlso OrElse
                combiner = token;
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

            if (left != null && right != null)
            {
                //at this point we have an operator because we have a left and a right
                var currentWorkingExpression = CreateComparisonOperator(left,
                                                                        right,
                                                                        operation ?? throw new NullReferenceException($"{nameof(operation)}"));

                //combiner would be && , ||
                workingExpression = combiner == null ?
                        currentWorkingExpression :
                        CreateCombinedExpressions(combiner,
                                                  workingExpression ?? throw new NullReferenceException($"{nameof(workingExpression)} Is Null"),
                                                  currentWorkingExpression ?? throw new NullReferenceException($"{nameof(currentWorkingExpression)} is null"));

                left = null;
                operation = null;
                right = null;
                combiner = null;
            }
        }

        return workingExpression ?? throw new Exception("No Expressions Found");
    }

    private static Expression CreateCombinedExpressions(Token binaryExpressionCombiner, Expression workingExpression, Expression currentExpression) =>
        binaryExpressionCombiner switch
        {
            AndAlsoToken => Expression.AndAlso(workingExpression, currentExpression),
            OrElseToken => Expression.OrElse(workingExpression, currentExpression),
            _ => throw new NotImplementedException()
        };

    private static Expression CreateComparisonOperator(Expression left, Expression right, Token operation) =>
        operation switch
        {
            GreaterThenToken => Expression.GreaterThan(left, right),
            GreaterThenOrEqualToken => Expression.GreaterThanOrEqual(left, right),
            EqualsToken => Expression.Equal(left, right),//Expression.Equal(Expression.Convert(left, right.Type), right),
            NotEqualsToken => Expression.NotEqual(left, right),
            LessThenToken => Expression.LessThan(left, right),
            LessThenOrEqualToken => Expression.LessThanOrEqual(left, right),
            _ => throw new NotImplementedException("Create Comparison Operator Not Build For Token Type = " + operation.GetType().Name),
        };
}
