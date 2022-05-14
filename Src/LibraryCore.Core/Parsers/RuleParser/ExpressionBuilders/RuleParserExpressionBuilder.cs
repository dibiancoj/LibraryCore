﻿using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using System.Collections.Immutable;
using System.Linq.Expressions;
using static LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation.ScoreToken;

namespace LibraryCore.Core.Parsers.RuleParser.ExpressionBuilders;

internal static class RuleParserExpressionBuilder
{
    internal static Expression CreateExpression(IImmutableList<IToken> tokens, IImmutableList<ParameterExpression> parametersToUse)
    {
        var (expressionStatements, expressionCombiners) = SortTree(tokens, parametersToUse);

        //only 1 statement. Just return it
        if (expressionStatements.Count == 1)
        {
            return expressionStatements.Dequeue();
        }

        Expression? finalExpression = null;

        while (expressionStatements.Count > 0)
        {
            var left = finalExpression ?? expressionStatements.Dequeue();
            var right = expressionStatements.Dequeue();

            finalExpression = expressionCombiners.Dequeue().CreateBinaryOperatorExpression(left, right);
        }

        return finalExpression ?? throw new Exception("Working Expression Is Null");
    }

    private static (Queue<Expression> ExpressionStatements, Queue<IBinaryExpressionCombiner> ExpressionCombiners) SortTree(IImmutableList<IToken> tokens, IImmutableList<ParameterExpression> parametersToUse)
    {
        var expressionStatements = new Queue<Expression>();
        var combinerStatements = new Queue<IBinaryExpressionCombiner>();

        Expression? statementLeft = null;
        Expression? statementRight = null;
        IBinaryComparisonToken? operation = null;

        foreach (var token in tokens.Where(x => x is not WhiteSpaceToken))
        {
            Expression temp = null!;

            if (token is IBinaryComparisonToken tempBinaryComparisonToken)
            {
                //==, !=, >, >=, <=
                operation = tempBinaryComparisonToken;
                continue;
            }
            else if (token is IBinaryExpressionCombiner tempBinaryExpressionCombiner)
            {
                if (operation == null || statementLeft == null || statementRight == null)
                {
                    throw new NullReferenceException("Operation || StatementLeft || Statement Right Is Null In IBinaryExpressionCombiner");
                }

                //since we lazy loaded the last statement..we load it in here now.
                expressionStatements.Enqueue(operation.CreateBinaryOperatorExpression(statementLeft, statementRight));

                //add the combiner statement now
                combinerStatements.Enqueue(tempBinaryExpressionCombiner);

                statementLeft = null;
                statementRight = null;
                operation = null;
                continue;
            }
            else if (token is IInstanceOperator instanceOperator)
            {
                var expressionToModifyBeforeClearing = statementLeft ?? statementRight ?? throw new Exception();

                if (statementRight != null)
                {
                    statementRight = null;
                }
                else
                {
                    statementLeft = null;
                }
                temp = instanceOperator.CreateInstanceExpression(parametersToUse, expressionToModifyBeforeClearing);
            }
            else
            {
                //normal clause
                temp = token.CreateExpression(parametersToUse);
            }

            if (statementLeft == null)
            {
                statementLeft = temp;
            }
            else if (statementRight == null)
            {
                statementRight = temp;
            }

            //if (statementLeft != null && statementRight != null)
            //{
            //    expressionStatements.Enqueue(operation!.CreateBinaryOperatorExpression(statementLeft, statementRight));
            //    statementLeft = null;
            //    statementRight = null;
            //}
        }

        //handle the last statement in the tree. It would never get created because it doesn't hit && or || and there is no whitespace at the end
        if (statementLeft != null && statementRight != null)
        {
            expressionStatements.Enqueue(operation!.CreateBinaryOperatorExpression(statementLeft, statementRight));
        }

        return (expressionStatements, combinerStatements);
    }

    internal static Expression CreateRuleExpression<TScoreResult>(ScoringMode scoringMode, IEnumerable<IToken> tokens, IImmutableList<ParameterExpression> parametersToUse)
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
