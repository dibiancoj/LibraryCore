namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories;

public class TokenFactoryProvider
{
    public TokenFactoryProvider(IEnumerable<ITokenFactory> tokenFactories)
    {
        TokenFactories = tokenFactories;
    }

    private IEnumerable<ITokenFactory> TokenFactories { get; }

    public ITokenFactory ResolveTokenFactory(char currentCharacterRead, char peekedCharacter, string readAndPeakedCharacters)
    {
        return TokenFactories.FirstOrDefault(x => x.IsToken(currentCharacterRead, peekedCharacter, readAndPeakedCharacters)) ?? throw new Exception($"No Token Found For Value = {currentCharacterRead}{peekedCharacter}");
    }

    public ITokenFactory ResolveSpecificFactory<TFactoryType>()
        where TFactoryType: ITokenFactory
    {
        return TokenFactories.OfType<TFactoryType>().First();
    }
}
