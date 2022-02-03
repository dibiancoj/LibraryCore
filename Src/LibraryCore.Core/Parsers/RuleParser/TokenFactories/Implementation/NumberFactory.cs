using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NumberFactory : ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeaked) => char.IsNumber(characterRead);

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        var text = new StringBuilder().Append(characterRead);

        while (stringReader.HasMoreCharacters() && char.IsNumber(stringReader.PeekCharacter()))
        {
            text.Append(stringReader.ReadCharacter());
        }

        return new NumberToken(Convert.ToInt32(text.ToString()));
    }

}

[DebuggerDisplay("{Value}")]
public record NumberToken(int Value) : Token
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters) => Expression.Constant(Value);
}