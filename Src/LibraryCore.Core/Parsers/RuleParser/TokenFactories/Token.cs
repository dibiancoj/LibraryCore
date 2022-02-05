using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories;

public abstract record Token
{
    public abstract Expression CreateExpression(IList<ParameterExpression> parameters);
}

/// <summary>
/// ==, !=, <=, >=, <, >
/// </summary>
public interface IBinaryComparisonToken : IBinaryOperator
{
}

/// <summary>
/// AndAlso, OrElse
/// </summary>
public interface IBinaryExpressionCombiner : IBinaryOperator
{
}

public interface IBinaryOperator
{
    public abstract Expression CreateBinaryOperatorExpression(Expression left, Expression right);
}