using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories;

public interface IToken
{
    Expression CreateExpression(IList<ParameterExpression> parameters);
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
    public Expression CreateInstanceExpression(IList<ParameterExpression> parameters, Expression instance);
}

public interface IBinaryOperator
{
    Expression CreateBinaryOperatorExpression(Expression left, Expression right);
}

public interface INumberToken
{
    Type NumberType { get; }
}