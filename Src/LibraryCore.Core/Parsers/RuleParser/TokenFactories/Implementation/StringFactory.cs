using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class StringFactory : ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '[';

    public Token CreateToken(char characterRead, StringReader stringReader)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != ']')
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
    public override Expression CreateExpression(ParameterExpression surveyParameter) => Expression.Constant(Value);
}