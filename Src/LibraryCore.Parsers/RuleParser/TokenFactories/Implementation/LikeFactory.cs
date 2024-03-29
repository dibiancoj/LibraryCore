﻿using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class LikeFactory : ITokenFactory
{
    private LikeToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "li", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        //read the l...ike
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "IKE");

        return CachedToken;
    }
}

[DebuggerDisplay("Like")]
public record LikeToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters) => throw new NotImplementedException();

    private static MethodInfo CachedStringContains => typeof(string).GetMethods()
                                                      .First(x => x.Name == nameof(string.Contains) && x.GetParameters().Length == 1);

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right)
    {
        return Expression.Call(left, CachedStringContains, right);
    }
}