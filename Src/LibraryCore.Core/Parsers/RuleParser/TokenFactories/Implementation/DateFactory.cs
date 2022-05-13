using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class DateFactory : ITokenFactory
{
    private const char DateTimeIdentifier = '^';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == DateTimeIdentifier;

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && IsInDateClause(stringReader))
        {
            text.Append(stringReader.ReadCharacter());
        }

        //eat the closing ^
        RuleParsingUtility.EatOrThrowCharacters(stringReader, new string(new[] { DateTimeIdentifier }));

        //we need to handle if this is nullable ('?')
        var typeToUse = IsNullableDate(stringReader) ? typeof(DateTime?) : typeof(DateTime);

        return CreateDateToken(typeToUse, text);
    }

    private static bool IsInDateClause(StringReader readerToUse)
    {
        //need to handle white spaces for the time
        return readerToUse.PeekCharacter() != DateTimeIdentifier;
    }

    private static bool IsNullableDate(StringReader readerToUse)
    {
        if (readerToUse.PeekCharacter() == RuleParsingUtility.NullableDataTypeIdentifier)
        {
            //eat the nullable type
            _ = readerToUse.Read();

            return true;
        }

        return false;
    }

    private static IToken CreateDateToken(Type typeToUse, StringBuilder textFound)
    {
        if (!DateTime.TryParse(textFound.ToString(), out DateTime tryToParseDateTime))
        {
            throw new Exception("Date Time Factory Not Able To Parse Date. Value = " + textFound.ToString());
        }

        return new DateToken(tryToParseDateTime, typeToUse);
    }
}

[DebuggerDisplay("Value = {Value} | Type = {TypeToUse.Name}")]
public record DateToken(DateTime Value, Type TypeToUse) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(Value, TypeToUse);
}