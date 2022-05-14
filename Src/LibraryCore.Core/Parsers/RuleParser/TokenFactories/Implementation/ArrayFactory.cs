using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class ArrayFactory : ITokenFactory
{
    //[1,2,3]
    //['string 1','string 2', 'string 3]

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == '[';

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != ']')
        {
            var parameterGroup = RuleParsingUtility.WalkTheParameterString(stringReader, tokenFactoryProvider, ']', ruleParserEngine).ToArray();

            return new ArrayToken(parameterGroup);
        }

        throw new Exception("ArrayFactory Has Blank Array Or Is Not Able To Parse The Value");
    }
}

[DebuggerDisplay("{Values}")]
public record ArrayToken(IEnumerable<IToken> Values) : IToken
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters)
    {
        var type = DetermineType();

        return Expression.NewArrayInit(type, Values.Select(x => x.CreateExpression(parameters)));
    }

    private Type DetermineType()
    {
        return Values.OfType<INumberToken>().FirstOrDefault()?.NumberType ?? typeof(string);
    }
}