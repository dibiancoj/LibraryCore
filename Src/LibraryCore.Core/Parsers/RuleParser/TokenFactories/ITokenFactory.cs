namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories;

public interface ITokenFactory
{
    bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters);
    IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider);
}

