using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Parsers.RuleParser.TokenFactories;

public interface IToken
{
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters);
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
    public Expression CreateInstanceExpression(IReadOnlyList<ParameterExpression> parameters, Expression instance);
}

public interface IBinaryOperator
{
    public Expression CreateBinaryOperatorExpression(Expression left, Expression right);
}

public interface INumberToken
{
    public Type NumberType { get; }
}