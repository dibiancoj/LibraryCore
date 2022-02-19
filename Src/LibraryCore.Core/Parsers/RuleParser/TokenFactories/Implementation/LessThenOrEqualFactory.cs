﻿using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class LessThenOrEqualFactory : ITokenFactory
{
    private LessThenOrEqualToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => readAndPeakedCharacters == "<=";

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the last =
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "=");

        return CachedToken;
    }

}

[DebuggerDisplay("<=")]
public record LessThenOrEqualToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.LessThanOrEqual(left, right);
}