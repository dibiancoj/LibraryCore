using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories;

public abstract record Token
{
    public abstract Expression CreateExpression(ParameterExpression surveyParameter);
}

/// <summary>
/// ==, <=, >=, <, >
/// </summary>
public interface IBinaryComparisonToken { }

/// <summary>
/// AndAlso, OrElse
/// </summary>
public interface IBinaryExpressionCombiner { }