using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;
using static System.Net.Mime.MediaTypeNames;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class EnumFactory : ITokenFactory
{
    /// <summary>
    /// Enum Value
    /// </summary>
    private const char TokenIdentifier = '#';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == TokenIdentifier;

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        //#MyEnumNamespace.Type|Smoking#
        var rawTypeInString = RuleParsingUtility.WalkUntil(stringReader, '|', true);

        //enum value
        var enumRawValue = RuleParsingUtility.WalkUntil(stringReader, TokenIdentifier, true);

        //is the last character a ?
        bool isNullable = enumRawValue[^1] == '?';

        if (isNullable)
        {
            //remove the ? if its there
            enumRawValue = enumRawValue[..^1];
        }

        var typeOfEnum = AppDomain.CurrentDomain.GetAssemblies()
                                .Where(a => !a.IsDynamic)
                                .SelectMany(a => a.GetTypes())
                                .First(t => t.FullName?.Equals(rawTypeInString) ?? false);

        return new EnumToken(enumRawValue, typeOfEnum, isNullable);
    }
}

[DebuggerDisplay("{Value}")]
public record EnumToken(string Value, Type TypeOfEnum, bool IsNullable) : IToken
{
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters)
    {
        if (!Enum.TryParse(TypeOfEnum, Value, out var tryToParseResult))
        {
            throw new Exception("Can't Parse Enum");
        }

        return IsNullable ?
            Expression.Constant(tryToParseResult, typeof(Nullable<>).MakeGenericType(TypeOfEnum)) :
            Expression.Constant(tryToParseResult, TypeOfEnum);
    }
}