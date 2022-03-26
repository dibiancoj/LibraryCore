using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class ParameterPropertyFactory : ITokenFactory
{
    private const char TokenIdentifier = '$';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == TokenIdentifier;

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != TokenIdentifier)
        {
            text.Append(stringReader.ReadCharacter());
        }

        //eat the closing $
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, TokenIdentifier);

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
public record ParameterPropertyToken(string? ParameterName, string PropertyName) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters)
    {
        //need to handle a few scenarios
        //A property off of a single parameter which is an object. ie: $MyParameter.Age$
        //A property which is an int (non-object). ie: $MyInt
        //Multiple parameters. ie: $Parameter1.Age and we have a $Parameter2$

        int howManyParameters = parameters.Count;

        //if the property name matches the parameter ie: $MyInt == 5. Then just add a reference to that parameter
        if (howManyParameters == 1 && parameters[0].Name == PropertyName)
        {
            return parameters[0];
        }

        //if this is not an object ie: $MyInt$....then just use the parameters
        if (ParameterName.IsNullOrEmpty())
        {
            return parameters.Single(x => x.Name?.Equals(PropertyName, StringComparison.OrdinalIgnoreCase) ?? throw new Exception("Property Name Not Found"));
        }

        //if there is 1 parameter then we know which parameter they want so they can just do $Age and we know its off of $MyParameter. 
        //if there is more then 1 we need to search for that parameter
        var parameterExpression = parameters.Single(x => x.Name.HasValue() && x.Name.Equals(ParameterName, StringComparison.OrdinalIgnoreCase));

        return Expression.PropertyOrField(parameterExpression, PropertyName);
    }
}
