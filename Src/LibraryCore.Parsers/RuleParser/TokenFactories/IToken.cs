using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Parsers.RuleParser.TokenFactories;

public interface IToken
{
    Expression CreateExpression(IImmutableList<ParameterExpression> parameters);
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

public interface IInstanceOperator
{
    public Expression CreateInstanceExpression(IImmutableList<ParameterExpression> parameters, Expression instance);
}

public interface IBinaryOperator
{
    Expression CreateBinaryOperatorExpression(Expression left, Expression right);
}

public interface INumberToken
{
    Type NumberType { get; }
}