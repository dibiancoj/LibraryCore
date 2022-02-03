﻿using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class GreaterThenOrEqualFactory : ITokenFactory
{
    private GreaterThenOrEqualToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '>' && characterPeaked == '=';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the last =
        _ = stringReader.Read();
        return CachedToken;
    }

}

[DebuggerDisplay(">=")]
public record GreaterThenOrEqualToken() : Token, IBinaryComparisonToken
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters) => throw new NotImplementedException();
}