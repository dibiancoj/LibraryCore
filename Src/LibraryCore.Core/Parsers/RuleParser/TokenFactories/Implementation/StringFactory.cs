using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class StringFactory : ITokenFactory
{
    /// <summary>
    /// Single quote
    /// </summary>
    private const char TokenIdentifier = '\'';

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == TokenIdentifier;

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != TokenIdentifier)
        {
            text.Append(stringReader.ReadCharacter());
        }

        //read the closing "]
        stringReader.Read();

        return new StringToken(text.ToString());
    }

}

[DebuggerDisplay("{Value}")]
public record StringToken(string Value) : Token
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters) => Expression.Constant(Value);
}