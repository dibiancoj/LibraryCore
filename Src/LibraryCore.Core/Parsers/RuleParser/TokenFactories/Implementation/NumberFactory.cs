using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NumberFactory : ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => char.IsNumber(characterRead);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        static bool IsFinalCharacter(StringReader readerToUse)
        {
            var peekedCharacter = readerToUse.PeekCharacter();

            return !char.IsWhiteSpace(peekedCharacter) && peekedCharacter != 'd' && peekedCharacter != '?';
        }

        var text = new StringBuilder().Append(characterRead);

        while (stringReader.HasMoreCharacters() && IsFinalCharacter(stringReader))
        {
            text.Append(stringReader.ReadCharacter());
        }

        var peekNextCharacter = stringReader.PeekCharacter();

        return peekNextCharacter == 'D' || peekNextCharacter == 'd' ?
            CreateDoubleToken(stringReader, text):
            CreateIntToken(stringReader, text);
    }

    private static IToken CreateDoubleToken(StringReader stringReader, StringBuilder textFound)
    {
        //remove the last character which is d
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'd');

        if (!double.TryParse(textFound.ToString(), out double number))
        {
            throw new Exception("Number Factory Not Able To Parse Number. Value = " + textFound.ToString());
        }

        return new NumberDoubleToken(number, RuleParsingUtility.DetermineNullableType<double, double?>(stringReader));
    }

    private static IToken CreateIntToken(StringReader stringReader, StringBuilder textFound)
    {
        if (!int.TryParse(textFound.ToString(), out int number))
        {
            throw new Exception("Number Factory Not Able To Parse Number. Value = " + textFound);
        }

        return new NumberToken(number, RuleParsingUtility.DetermineNullableType<int, int?>(stringReader));
    }
}

[DebuggerDisplay("{Value} | Type = {TypeToUse}")]
public record NumberToken(int Value, Type TypeToUse) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(Value, TypeToUse);
}

[DebuggerDisplay("{Value} | Type = {TypeToUse}")]
public record NumberDoubleToken(double Value, Type TypeToUse) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(Value, TypeToUse);
}