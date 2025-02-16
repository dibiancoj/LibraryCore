using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories;

public interface ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters);
    public IToken CreateToken(char characterRead, StringReader stringReader, CreateTokenParameters createTokenParameters);
}