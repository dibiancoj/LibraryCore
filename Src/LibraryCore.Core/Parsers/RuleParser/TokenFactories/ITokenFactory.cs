namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories;

public interface ITokenFactory
{
    bool IsToken(char characterRead, char characterPeeked);
    Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider);
}

