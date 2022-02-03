﻿using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NotEqualsFactory : ITokenFactory
{
    private NotEqualsToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '!' && characterPeaked == '=';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the other equals
        _ = stringReader.Read();
        return CachedToken;
    }
}

[DebuggerDisplay("!=")]
public record NotEqualsToken() : Token, IBinaryComparisonToken
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters) => throw new NotImplementedException();
}