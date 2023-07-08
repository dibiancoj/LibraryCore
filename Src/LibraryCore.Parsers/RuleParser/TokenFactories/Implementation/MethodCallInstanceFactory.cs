using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class MethodCallInstanceFactory : ITokenFactory
{
    //[1,2,3].Any($x$ => $x$ > 2) == true
    //[1,2,3].Count($x$ => $x$ > 2) >= 1
    //[1,2,3].Where($x$ => $x$ > 100).Any($x$ => $x$ == 1)

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == '.';

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        return new MethodCallInstanceToken(RuleParsingUtility.ParseMethodSignature(stringReader, createTokenParameters));
    }
}

[DebuggerDisplay("Method Name = {MethodInformation.MethodName}")]
public record MethodCallInstanceToken(RuleParsingUtility.MethodParsingResult MethodInformation) : IToken, IInstanceOperator
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateInstanceExpression(IImmutableList<ParameterExpression> parameters, Expression instance)
    {
        Type typeToFetchMethodInfoOffOf = typeof(IEnumerable).IsAssignableFrom(instance.Type) && instance.Type != typeof(string) ?
                                                typeof(Enumerable) :
                                                instance.Type;

        var allMethods = typeToFetchMethodInfoOffOf.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance)
                                .Where(x => x.Name.Equals(MethodInformation.MethodName, StringComparison.OrdinalIgnoreCase));

        var isExtensionMethod = allMethods.Any(t => t.IsDefined(typeof(ExtensionAttribute))) ? 1 : 0;

        var methodInfoToCall = allMethods
                               .First(x => x.GetParameters().Length == isExtensionMethod + MethodInformation.Parameters.Count);

        if (methodInfoToCall.IsGenericMethod)
        {
            methodInfoToCall = methodInfoToCall.MakeGenericMethod(RuleParsingUtility.DetermineGenericType(instance));
        }

        if (methodInfoToCall.IsStatic)
        {
            //this would be an extension method. Or a generic extension method
            var genericParameters = MethodInformation.Parameters.Select(x => CreateMethodParameter(instance, x, parameters));

            return Expression.Call(methodInfoToCall, new Expression[] { instance }.Concat(genericParameters));
        }
        else
        {
            return Expression.Call(instance, methodInfoToCall, MethodInformation.Parameters.Select(x => CreateMethodParameter(null, x, parameters)));
        }
    }

    private static Expression CreateMethodParameter(Expression? instance, IToken token, IImmutableList<ParameterExpression> parameters)
    {
        return token is IInstanceOperator instanceOperator ?
                    instanceOperator.CreateInstanceExpression(parameters, instance!) :
                     token.CreateExpression(parameters);
    }
}