using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using System.Linq.Expressions;
using static LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation.ScoreToken;

namespace LibraryCore.Core.Parsers.RuleParser.ExpressionBuilders;

internal static class RuleParserExpressionBuilder
{
    internal static Expression CreateExpression(IEnumerable<IToken> tokens, ParameterExpression[] parametersToUse)
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

    internal static Expression CreateRuleExpression<TScoreResult>(ScoringMode scoringMode, IEnumerable<IToken> tokens, ParameterExpression[] parametersToUse)
    {
        var workingExpressions = new List<Expression>();

        var returnTarget = Expression.Label();
        var resultVariable = Expression.Variable(typeof(TScoreResult));

        var firstScoreWins = scoringMode == ScoringMode.ShortCircuitOnFirstTrueEval;

        foreach (var rule in tokens.Cast<ScoreCriteriaToken<TScoreResult>>())
        {
            Expression scoreTallyExpression = firstScoreWins ?
                                            Expression.Constant(rule.ScoreValue) :
                                            Expression.Add(resultVariable, Expression.Constant(rule.ScoreValue));

            var conditionToRun = CreateExpression(rule.ScoreCriteriaTokens, parametersToUse);
            var assignExpression = Expression.Assign(resultVariable, scoreTallyExpression);

            //first score wins
            //set variable, return methoid

            //accumulator
            //set variable

            Expression expressionBasedOnScoreType = firstScoreWins ?
                                Expression.Block(assignExpression, Expression.Return(returnTarget)) :
                                assignExpression;

            workingExpressions.Add(Expression.IfThen(conditionToRun, expressionBasedOnScoreType));
        }

        workingExpressions.Add(Expression.Label(returnTarget));
        workingExpressions.Add(resultVariable);

        return Expression.Block(new[] { resultVariable }, workingExpressions);
    }
}
