using LibraryCore.Parsers.RuleParser;
using LibraryCore.Parsers.RuleParser.ExpressionBuilders;
using LibraryCore.Parsers.RuleParser.TokenFactories;
using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class LambdaFactory : ITokenFactory
{
    //[1,2,3].Any($x$ => $x$ > 2) == true
    //[1,2,3].Count($x$ => $x$ > 2) >= 1
    //[1,2,3].Where($x$ => $x$ > 100).Any($x$ => $x$ == 1)

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => readAndPeakedCharacters.Contains("=>");

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              TokenFactoryProvider tokenFactoryProvider,
                              RuleParserEngine ruleParserEngine,
                              SchemaModel schema)
    {
        //parsing:
        //$x$ => $x$ == 2

        var allParameters = RuleParsingUtility.WalkUntil(stringReader, '=')
                                .Trim()
                                .Replace("$", string.Empty)
                                .Split(',');

        RuleParsingUtility.EatOrThrowCharacters(stringReader, "=>");

        //grab the body
        var bodyOfMethod = RuleParsingUtility.WalkUntilEof(stringReader);

        var tokensInBody = ruleParserEngine.ParseString(bodyOfMethod);

        return new LambdaToken(allParameters.ToImmutableList(), tokensInBody.CompilationTokenResult, schema);
    }
}

[DebuggerDisplay("Inline Lambda")]
public record LambdaToken(IImmutableList<string> MethodParameters, IImmutableList<IToken> MethodBodyTokens, SchemaModel Schema) : IToken, IInstanceOperator
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateInstanceExpression(IImmutableList<ParameterExpression> parameters, Expression instance)
    {
        Type genericType = RuleParsingUtility.DetermineGenericType(instance);

        //x in the x => 
        var funcParameter = Expression.Parameter(genericType, MethodParameters[0]);

        var funcParameterArray = new[] { funcParameter }.ToImmutableList();

        var functionBodyToExecute = RuleParserExpressionBuilder.CreateExpression(MethodBodyTokens, funcParameterArray, Schema);

        return Expression.Lambda(functionBodyToExecute, funcParameterArray);
    }
}