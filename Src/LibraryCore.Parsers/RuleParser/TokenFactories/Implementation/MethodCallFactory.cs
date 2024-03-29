﻿using LibraryCore.Parsers.RuleParser.Utilities;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class MethodCallFactory : ITokenFactory
{
    private Dictionary<string, MethodInfo> RegisterdMethods { get; } = [];

    public MethodCallFactory RegisterNewMethodAlias(string key, MethodInfo registeredMethodParameters)
    {
        RegisterdMethods.Add(key, registeredMethodParameters);
        return this;
    }

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == '@';

    //@MyMethod(1)
    //@MyMethod(1,'abc', true)

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        var methodInfoSignature = RuleParsingUtility.ParseMethodSignature(stringReader, createTokenParameters);

        if (!RegisterdMethods.TryGetValue(methodInfoSignature.MethodName, out var tryToGetMethodInfoResult))
        {
            throw new Exception($"Method Name = {methodInfoSignature.MethodName} Is Not Registered In MethodCallFactory. Call {nameof(RegisterNewMethodAlias)} To Register The Method");
        }

        return new MethodCallToken(tryToGetMethodInfoResult, methodInfoSignature.Parameters);
    }
}

[DebuggerDisplay("Method Call {RegisteredMethodToUse}")]
public record MethodCallToken(MethodInfo RegisteredMethodToUse, IReadOnlyList<IToken> AdditionalParameters) : IToken
{
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters)
    {
        //convert all the additional parameters to an expression
        var parameterExpression = AdditionalParameters.Select(x => x.CreateExpression(parameters)).ToArray();

        return Expression.Call(RegisteredMethodToUse, parameterExpression);
    }
}