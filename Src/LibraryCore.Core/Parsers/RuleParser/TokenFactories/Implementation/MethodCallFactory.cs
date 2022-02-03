using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class MethodCallFactory : ITokenFactory
{
    private Dictionary<string, MethodInfo> RegisterdMethods { get; } = new();

    public MethodCallFactory RegisterNewMethodAlias(string key, MethodInfo registeredMethodParameters)
    {
        RegisterdMethods.Add(key, registeredMethodParameters);
        return this;
    }

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '@';

    //@MyMethod(1)
    //@MyMethod(1,[abc], true)

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
        {
            //need to determine the method name so we walk 
            var methodName = WalkTheMethodName(stringReader);
            var parameterGroup = WalkTheParameterString(stringReader, tokenFactoryProvider).ToArray();

            return new MethodCallToken(RegisterdMethods[methodName], parameterGroup);
        }

        throw new Exception("MethodCallFactory Not Able To Parse Information");
    }

    private static string WalkTheMethodName(StringReader reader)
    {
        var text = new StringBuilder();

        while (reader.HasMoreCharacters() && reader.PeekCharacter() != '(')
        {
            text.Append(reader.ReadCharacter());
        }

        return text.ToString();
    }

    private static IEnumerable<Token> WalkTheParameterString(StringReader reader, TokenFactoryProvider tokenFactoryProvider)
    {
        var text = new StringBuilder();

        //eat the opening (
        _ = reader.Read();

        //eat until the end of the method
        while (reader.HasMoreCharacters() && reader.PeekCharacter() != ')')
        {
            text.Append(reader.ReadCharacter());
        }

        //eat the closing )
        _ = reader.Read();

        foreach(var parameter in text.ToString().Split(','))
        {
            using var parameterReader = new StringReader(parameter.Trim());

            var characterRead = parameterReader.ReadCharacter();
            var nextPeekedCharacter = parameterReader.PeekCharacter();

            yield return tokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter).CreateToken(characterRead, parameterReader, tokenFactoryProvider);
        }
    }
}

[DebuggerDisplay("Method Call {RegisteredMethodToUse}")]
public record MethodCallToken(MethodInfo RegisteredMethodToUse, IEnumerable<Token> AdditionalParameters) : Token
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters)
    {
        //convert all the additional parameters to an expression
        var additionalParameterExpression = AdditionalParameters.Select(x => x.CreateExpression(parameters)).ToArray();

        //parameters = The overall method parameters that was created
        //additional parameters = is whatever we want to call this method with
        return Expression.Call(RegisteredMethodToUse, parameters.Concat(additionalParameterExpression));
    }
}