using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class ParameterPropertyFactory : ITokenFactory
{
    private const char TokenIdentifier = '$';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == TokenIdentifier;

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != TokenIdentifier)
        {
            text.Append(stringReader.ReadCharacter());
        }

        //eat the closing $
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, TokenIdentifier);

        return new ParameterPropertyToken(text.ToString().Split('.'));
    }
}

[DebuggerDisplay("Parameter Property Path = {DebuggerDisplay()}")]
public record ParameterPropertyToken(IList<string> PropertyPath) : IToken
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters)
    {
        //need to handle a few scenarios
        //A property off of a single parameter which is an object. ie: $MyParameter.Age$
        //A property which is an int (non-object). ie: $MyInt
        //Multiple parameters. ie: $Parameter1.Age and we have a $Parameter2$

        //loop through each level and keep grabbing the next level
        Expression workingExpression = parameters.Single(x => x.Name == PropertyPath[0]);

        foreach (var propertyLevel in PropertyPath.Skip(1))
        {
            workingExpression = Expression.PropertyOrField(workingExpression, propertyLevel);
        }

        return workingExpression;
    }

    private string DebuggerDisplay() => string.Join('.', PropertyPath);

}
