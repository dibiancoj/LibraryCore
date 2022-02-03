namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories;

public class TokenFactoryProvider
{
    public TokenFactoryProvider(IEnumerable<ITokenFactory> tokenFactories)
    {
        TokenFactories = tokenFactories;
    }

    private IEnumerable<ITokenFactory> TokenFactories { get; }

    public ITokenFactory ResolveTokenFactory(char currentCharacterRead, char peekedCharacter)
    {
        return TokenFactories.FirstOrDefault(x => x.IsToken(currentCharacterRead, peekedCharacter)) ?? throw new Exception($"No Token Found For Value = {currentCharacterRead}{peekedCharacter}");
    }
}
