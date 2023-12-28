using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class NumberFactory : ITokenFactory
{
    private const char DoubleTokenIdentifier = 'd';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => char.IsNumber(characterRead);

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        var text = new StringBuilder().Append(characterRead);

        while (stringReader.HasMoreCharacters() && IsFinalCharacter(stringReader))
        {
            text.Append(stringReader.ReadCharacter());
        }

        //we need to handle if this is nullable or a double (double = 'd', nullable = '?')
        var (IsDoubleDataType, IsNullable) = WalkAdditionalCharacters(stringReader);

        var typeToUse = DetermineType(IsDoubleDataType, IsNullable);

        return IsDoubleDataType ?
            CreateDoubleToken(typeToUse, text) :
            CreateIntToken(typeToUse, text);
    }

    private static Type DetermineType(bool isDouble, bool isNullable)
    {
        if (isDouble)
        {
            return isNullable ?
                        typeof(double?) :
                        typeof(double);
        }

        return isNullable ?
            typeof(int?) :
            typeof(int);
    }

    private static bool IsFinalCharacter(StringReader readerToUse)
    {
        var peekedCharacter = readerToUse.PeekCharacter();

        return !char.IsWhiteSpace(peekedCharacter) && peekedCharacter != DoubleTokenIdentifier && peekedCharacter != RuleParsingUtility.NullableDataTypeIdentifier;
    }

    private static (bool IsDoubleDataType, bool IsNullable) WalkAdditionalCharacters(StringReader stringReader)
    {
        //after a number you can specify:
        //? = nullable
        //d = double

        //so after the number is done..see if the next characters are either of those then create the expression based on that type (nullable and is double or int)

        var peekNextCharacter = stringReader.PeekCharacter();

        if (peekNextCharacter != DoubleTokenIdentifier && peekNextCharacter != RuleParsingUtility.NullableDataTypeIdentifier)
        {
            return (false, false);
        }

        bool isDouble = false;
        bool isNullable = false;

        //walk until the end of the string or a space which is the real end of this number
        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
        {
            var readCharacter = stringReader.ReadCharacter();

            if (readCharacter == DoubleTokenIdentifier)
            {
                isDouble = true;
            }
            else if (readCharacter == RuleParsingUtility.NullableDataTypeIdentifier)
            {
                isNullable = true;
            }
        }

        return (isDouble, isNullable);
    }

    private static IToken CreateDoubleToken(Type typeToUse, StringBuilder textFound)
    {
        if (!double.TryParse(textFound.ToString(), out double tryToParseNumber))
        {
            throw new Exception("Number Factory [Double] Not Able To Parse Number. Value = " + textFound.ToString());
        }

        return new NumberToken<double>(tryToParseNumber, typeToUse);
    }

    private static IToken CreateIntToken(Type typeToUse, StringBuilder textFound)
    {
        if (!int.TryParse(textFound.ToString(), out int tryToParseNumber))
        {
            throw new Exception("Number Factory [Int] Not Able To Parse Number. Value = " + textFound.ToString());
        }

        return new NumberToken<int>(tryToParseNumber, typeToUse);
    }
}

[DebuggerDisplay("Value = {Value} | Type = {NumberType.Name}")]
public record NumberToken<T>(T Value, Type NumberType) : IToken, INumberToken
{
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters) => Expression.Constant(Value, NumberType);
}