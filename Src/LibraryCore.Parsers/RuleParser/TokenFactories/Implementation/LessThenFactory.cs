﻿using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class LessThenFactory : ITokenFactory
{
    private LessThenToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == '<' && characterPeeked != '=';

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              TokenFactoryProvider tokenFactoryProvider,
                              RuleParserEngine ruleParserEngine,
                              SchemaModel schema) => CachedToken;
}

[DebuggerDisplay("<")]
public record LessThenToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.LessThan(left, right);
}
