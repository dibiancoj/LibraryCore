using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class ParameterPropertyFactory : ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeeked) => characterRead == '$';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
        {
            text.Append((char)stringReader.Read());
        }

        var finalValue = text.ToString();

        if (finalValue.Contains('.'))
        {
            var splitText = finalValue.Split('.');

            return new ParameterPropertyToken(splitText[0], splitText[1]);
        }

        return new ParameterPropertyToken(null, finalValue);
    }
}

[DebuggerDisplay("Parameter Property Name = {ParameterName}.{PropertyName}")]
public record ParameterPropertyToken(string? ParameterName, string PropertyName) : Token
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters)
    {
        //need to handle a few scenarios
        //A property off of a single parameter which is an object. ie: $MyParameter.Age
        //A property which is an int (non-object). ie: $MyInt
        //Multiple parameters. ie: $Parameter1.Age and we have a $Parameter2

        int howManyParameters = parameters.Count;

        //if the property name matches the parameter ie: $MyInt == 5. Then just add a reference to that parameter
        if (howManyParameters == 1 && parameters[0].Name == PropertyName)
        {
            return parameters[0];
        }

        //if there is 1 parameter then we know which parameter they want so they can just do $Age and we know its off of $MyParameter. 
        //if there is more then 1 we need to search for that parameter
        var parameterExpression = howManyParameters == 1 ?
                                                        parameters[0] :
                                                        parameters.SingleOrDefault(x => x.Name.HasValue() && x.Name.Equals(ParameterName, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception($"Parameter Name Not Found: {ParameterName}");

        return Expression.PropertyOrField(parameterExpression, PropertyName);
    }
}
