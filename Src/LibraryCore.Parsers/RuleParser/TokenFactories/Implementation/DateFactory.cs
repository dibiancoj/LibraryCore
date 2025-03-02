﻿using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class DateFactory : ITokenFactory
{
    private const char DateTimeIdentifier = '^';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == DateTimeIdentifier;

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && IsInDateClause(stringReader))
        {
            text.Append(stringReader.ReadCharacter());
        }

        //eat the closing ^
        RuleParsingUtility.EatOrThrowCharacters(stringReader, new string([DateTimeIdentifier]));

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

    private static DateToken CreateDateToken(Type typeToUse, StringBuilder textFound)
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
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters) => Expression.Constant(Value, TypeToUse);
}