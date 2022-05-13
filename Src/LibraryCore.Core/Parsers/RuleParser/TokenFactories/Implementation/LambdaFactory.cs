using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.ExpressionBuilders;
using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class LambdaFactory : ITokenFactory
{
    //[1,2,3].Any($x$ => $x$ > 2) == true
    //[1,2,3].Count($x$ => $x$ > 2) >= 1
    //[1,2,3].Where($x$ => $x$ > 100).Any($x$ => $x$ == 1)

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => readAndPeakedCharacters.Contains("=>");

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        var allParameters = RuleParsingUtility.WalkUntil(stringReader, '=').Trim().Replace("$", string.Empty).Split(',');

        RuleParsingUtility.EatOrThrowCharacters(stringReader, "=>");

        //grab the body
        var bodyOfMethod = RuleParsingUtility.WalkUntilEof(stringReader);

        var tokensInBody = ruleParserEngine.ParseString(bodyOfMethod);

        return new LambdaToken(allParameters.ToImmutableList(), tokensInBody.CompilationTokenResult);
    }
}

[DebuggerDisplay("Inline Lambda")]
public record LambdaToken(IImmutableList<string> MethodParameters, IImmutableList<IToken> MethodBodyTokens) : IToken, IInstanceOperator
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateInstanceExpression(IList<ParameterExpression> parameters, Expression instance)
    {
        throw new Exception();
        //var zz = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
        //        .Where(m => m.Name == MethodName)
        //        .ToList();

        //var isExtensionMethod = zz.Any(t => t.IsDefined(typeof(ExtensionAttribute))) ? 1 : 0;

        //var z = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
        //   .First(m => m.Name.Equals(MethodName, StringComparison.OrdinalIgnoreCase) && 
        //          m.GetParameters().Length == isExtensionMethod + MethodParameters.Count);

        ////if (!instance.Type.IsArray || (instance.Type.IsGenericType && instance.Type.GetGenericTypeDefinition() != typeof(IEnumerable<>)))
        ////{
        ////    throw new Exception("Must Be An Array To Use Linq Call");
        ////}

        //Type typeToUse = (instance.Type.IsGenericType ?
        //                instance.Type.GenericTypeArguments[0] :
        //                instance.Type.GetElementType()) ?? throw new Exception();

        //var MethodInGenericType = z.MakeGenericMethod(typeToUse);

        //var funcParameter = Expression.Parameter(typeToUse, MethodParameters[0]);
        //var funcParameterArray = new[] { funcParameter };

        //var whereBla = RuleParserExpressionBuilder.CreateExpression(MethodBodyTokens, funcParameterArray);


        ////var ttt = Expression.Lambda<Func<object, bool>>(whereBla, funcparameterArray);
        //var ttt = Expression.Lambda(whereBla, funcParameterArray);

        //return Expression.Call(MethodInGenericType, new[] { instance, ttt });
    }
}