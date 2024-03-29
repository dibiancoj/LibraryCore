﻿using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class LessThenOrEqualFactory : ITokenFactory
{
    private LessThenOrEqualToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => readAndPeakedCharacters == "<=";

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        //read the last =
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "=");

        return CachedToken;
    }

}

[DebuggerDisplay("<=")]
public record LessThenOrEqualToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.LessThanOrEqual(left, right);
}