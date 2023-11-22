namespace LibraryCore.Parsers.RuleParser.TokenFactories;

public class TokenFactoryProvider(IEnumerable<ITokenFactory> tokenFactories)
{
    public ITokenFactory ResolveTokenFactory(char currentCharacterRead, char peekedCharacter, string readAndPeakedCharacters)
    {
        return tokenFactories.FirstOrDefault(x => x.IsToken(currentCharacterRead, peekedCharacter, readAndPeakedCharacters)) ?? throw new Exception($"No Token Found For Value = {currentCharacterRead}{peekedCharacter}");
    }

    public ITokenFactory ResolveSpecificFactory<TFactoryType>()
        where TFactoryType : ITokenFactory
    {
        return tokenFactories.OfType<TFactoryType>().First();
    }
}
