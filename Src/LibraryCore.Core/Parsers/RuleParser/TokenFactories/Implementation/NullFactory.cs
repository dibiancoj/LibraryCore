﻿using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NullTokenFactory : ITokenFactory
{
    private NullToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == 'n' && characterPeeked == 'u';

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the ull
        stringReader.EatXNumberOfCharacters(3);

        return CachedToken;
    }
}

[DebuggerDisplay("null")]
public record NullToken() : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(null);
}