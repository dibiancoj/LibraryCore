using LibraryCore.Parsers.RuleParser.Utilities;
using System.Text.Json;

namespace LibraryCore.Parsers.RuleParser.TokenFactories;

public interface ITokenFactory
{
    bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters);
    IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine, SchemaModel schema);
}

