﻿using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NullTokenFactory : ITokenFactory
{
    private NullToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == 'n' && characterPeaked == 'u';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the ull
        _ = stringReader.Read();
        _ = stringReader.Read();
        _ = stringReader.Read();

        return CachedToken;
    }
}

[DebuggerDisplay("null")]
public record NullToken() : Token
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(null);
}