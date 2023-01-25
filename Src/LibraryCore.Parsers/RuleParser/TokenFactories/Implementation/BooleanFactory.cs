using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class BooleanFactory : ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "fa", StringComparison.OrdinalIgnoreCase) ||
                                                                                                     string.Equals(readAndPeakedCharacters, "tr", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        //set the value if its true / false.
        bool valueToUse = characterRead == 't' || characterRead == 'T';

        //skip the first character since we already read it. Then eat the rest of the word
        RuleParsingUtility.EatOrThrowCharacters(stringReader, valueToUse.ToString()[1..]);

        return new BooleanToken(valueToUse, RuleParsingUtility.DetermineNullableType<bool, bool?>(stringReader));
    }
}

[DebuggerDisplay("Value = {Value}")]
public record BooleanToken(bool Value, Type TypeToUse) : IToken
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => Expression.Constant(Value, TypeToUse);
}