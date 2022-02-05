namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories;

public interface ITokenFactory
{
    bool IsToken(char characterRead, char characterPeeked);
    IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider);
}

