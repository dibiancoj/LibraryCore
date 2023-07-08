using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories;

public interface ITokenFactory
{
    bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters);
    IToken CreateToken(char characterRead, StringReader stringReader, CreateTokenParameters createTokenParameters);
}