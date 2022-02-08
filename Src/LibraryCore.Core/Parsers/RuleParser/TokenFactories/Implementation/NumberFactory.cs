using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NumberFactory : ITokenFactory
{
    private const char NullableTokenIdentifier = '?';
    private const char DoubleTokenIdentifier = 'd';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => char.IsNumber(characterRead);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
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

        return !char.IsWhiteSpace(peekedCharacter) && peekedCharacter != DoubleTokenIdentifier && peekedCharacter != NullableTokenIdentifier;
    }

    private static (bool IsDoubleDataType, bool IsNullable) WalkAdditionalCharacters(StringReader stringReader)
    {
        //after a number you can specify:
        //? = nullable
        //d = double

        //so after the number is done..see if the next characters are either of those then create the expression based on that type (nullable and is double or int)

        var peekNextCharacter = stringReader.PeekCharacter();

        if (peekNextCharacter != DoubleTokenIdentifier && peekNextCharacter != NullableTokenIdentifier)
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
            else if (readCharacter == NullableTokenIdentifier)
            {
                isNullable = true;
            }
        }

        return (isDouble, isNullable);
    }

    private static IToken CreateDoubleToken(Type typeToUse, StringBuilder textFound)
    {
        if (!double.TryParse(textFound.ToString(), out double number))
        {
            throw new Exception("Number Factory Not Able To Parse Number. Value = " + textFound.ToString());
        }

        return new NumberDoubleToken(number, typeToUse);
    }

    private static IToken CreateIntToken(Type typeToUse, StringBuilder textFound)
    {
        if (!int.TryParse(textFound.ToString(), out int number))
        {
            throw new Exception("Number Factory Not Able To Parse Number. Value = " + textFound);
        }

        return new NumberToken(number, typeToUse);
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