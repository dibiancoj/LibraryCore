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

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == '@';

    //@MyMethod(1)
    //@MyMethod(1,'abc', true)

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
        {
            //need to determine the method name so we walk 
            var methodName = WalkTheMethodName(stringReader);

            //eat the opening (
            stringReader.EatXNumberOfCharacters(1);

            bool hasNoParameters = stringReader.PeekCharacter() == ')';

            var parameterGroup = hasNoParameters ?
                                        Enumerable.Empty<Token>() :
                                        RuleParsingUtility.WalkTheParameterString(stringReader, tokenFactoryProvider, ')').ToArray();

            if (hasNoParameters)
            {
                //closing )
                stringReader.Read();
            }

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

}

[DebuggerDisplay("Method Call {RegisteredMethodToUse}")]
public record MethodCallToken(MethodInfo RegisteredMethodToUse, IEnumerable<Token> AdditionalParameters) : Token
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters)
    {
        //convert all the additional parameters to an expression
        var additionalParameterExpression = AdditionalParameters.Select(x => x.CreateExpression(parameters)).ToArray();

        IEnumerable<Expression> parametersToPassIn = RegisteredMethodToUse.GetParameters().Any() ?
                                                    parameters.Concat(additionalParameterExpression) :
                                                    Enumerable.Empty<Expression>();

        //parameters = The overall method parameters that was created
        //additional parameters = is whatever we want to call this method with
        return Expression.Call(RegisteredMethodToUse, parametersToPassIn);
    }
}