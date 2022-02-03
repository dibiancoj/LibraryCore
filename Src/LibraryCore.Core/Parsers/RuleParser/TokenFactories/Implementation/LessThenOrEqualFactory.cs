﻿using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class LessThenOrEqualFactory : ITokenFactory
{
    private LessThenOrEqualToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '<' && characterPeaked == '=';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the last =
        stringReader.EatXNumberOfCharacters(1);
        return CachedToken;
    }

}

[DebuggerDisplay("<=")]
public record LessThenOrEqualToken() : Token, IBinaryComparisonToken
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.LessThanOrEqual(left, right);
}