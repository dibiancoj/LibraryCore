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
        var text = new StringBuilder().Append(characterRead);

        while (stringReader.HasMoreCharacters() && IsFinalCharacter(stringReader))
        {
            text.Append(stringReader.ReadCharacter());
        }

        //we need to handle if this is nullable or a double (double = 'd', nullable = '?')
        var (IsDouble, IsNullable) = WalkAdditionalCharacters(stringReader);

        var typeToUse = DetermineType(IsDouble, IsNullable);

        return IsDouble ?
            CreateDoubleToken(typeToUse, text) :
            CreateIntToken(typeToUse, text);
    }

    private static Type DetermineType(bool isDouble, bool isNullable)
    {
        if (isDouble && isNullable)
        {
            return typeof(double?);
        }

        if (isDouble)
        {
            return typeof(double);
        }

        if (isNullable)
        {
            return typeof(int?);
        }

        return typeof(int);
    }

    private static bool IsFinalCharacter(StringReader readerToUse)
    {
        var peekedCharacter = readerToUse.PeekCharacter();

        return !char.IsWhiteSpace(peekedCharacter) && peekedCharacter != 'd' && peekedCharacter != '?';
    }

    private static (bool IsDouble, bool IsNullable) WalkAdditionalCharacters(StringReader stringReader)
    {
        var peekNextCharacter = stringReader.PeekCharacter();

        if (peekNextCharacter != 'd' && peekNextCharacter != '?')
        {
            return (false, false);
        }

        bool isDouble = false;
        bool isNullable = false;

        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
        {
            var readCharacter = stringReader.ReadCharacter();

            if (readCharacter == 'd')
            {
                isDouble = true;
            }
            else if (readCharacter == '?')
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