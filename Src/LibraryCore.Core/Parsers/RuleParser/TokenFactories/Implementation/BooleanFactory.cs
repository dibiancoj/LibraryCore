using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class BooleanFactory : ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "fa", StringComparison.OrdinalIgnoreCase) ||
                                                                                                     string.Equals(readAndPeakedCharacters, "tr", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        bool valueToUse;

        if (characterRead == 'f' || characterRead == 'F')
        {
            valueToUse = false;

            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'A', 'a');
            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'L', 'l');
            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'S', 's');
            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'E', 'e');
        }
        else
        {
            valueToUse = true;
            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'R', 'r');
            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'U', 'u');
            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'E', 'e');
        }

        return new BooleanToken(valueToUse, RuleParsingUtility.DetermineNullableType<bool, bool?>(stringReader));
    }
}

[DebuggerDisplay("Value = {Value}")]
public record BooleanToken(bool Value, Type TypeToUse) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(Value, TypeToUse);
}