using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NumberFactory : ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeeked) => char.IsNumber(characterRead);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        var text = new StringBuilder().Append(characterRead);

        while (stringReader.HasMoreCharacters() && char.IsNumber(stringReader.PeekCharacter()))
        {
            text.Append(stringReader.ReadCharacter());
        }

        if (!int.TryParse(text.ToString(), out int number))
        {
            throw new Exception("Number Factory Not Able To Parse Number. Value = " + text.ToString());
        }

        return new NumberToken(number);
    }

}

[DebuggerDisplay("{Value}")]
public record NumberToken(int Value) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(Value);
}