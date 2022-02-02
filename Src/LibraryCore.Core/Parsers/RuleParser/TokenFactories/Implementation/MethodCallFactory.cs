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

    public Token CreateToken(char characterRead, StringReader stringReader)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
        {
            //need to determine the method name so we walk 
            var methodName = WalkTheString(stringReader, '(');
            var parameterGroup = WalkTheString(stringReader, ')');

            var parameterExpressions = new List<Expression>();

            foreach (var parameter in parameterGroup.Split(','))
            {
                
            }

            text.Append(stringReader.ReadCharacter());
        }

        return new MethodCallToken(RegisterdMethods[text.ToString()]);
    }

    private static string WalkTheString(StringReader reader, char walkUntilThisCharacter)
    {
        var text = new StringBuilder();

        while (reader.HasMoreCharacters() && reader.PeekCharacter() != walkUntilThisCharacter)
        {
            text.Append(reader.ReadCharacter());
        }

        return text.ToString();
    }
}

[DebuggerDisplay("Method Call {RegisteredMethodToUse.MethodInfoToCall}")]
public record MethodCallToken(MethodInfo RegisteredMethodToUse, IEnumerable<Expression> AdditionalParameters) : Token
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters)
    {
        //parameters = The overall method parameters that was created
        //additional parameters = is whatever we want to call this method with
        return Expression.Call(RegisteredMethodToUse, parameters.Concat(AdditionalParameters));
    }
}