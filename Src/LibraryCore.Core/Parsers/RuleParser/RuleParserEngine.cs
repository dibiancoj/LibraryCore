using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using System.Collections.Immutable;

namespace LibraryCore.Core.Parsers.RuleParser;

public class RuleParserEngine
{
    public RuleParserEngine(IEnumerable<ITokenFactory> tokenFactories)
    {
        TokenFactories = tokenFactories;
    }

    private IEnumerable<ITokenFactory> TokenFactories { get; }

    public IImmutableList<Token> ParseString(string stringToParse)
    {
        using var reader = new StringReader(stringToParse);
        var tokens = new List<Token>();

        while (reader.HasMoreCharacters())
        {
            var characterRead = reader.ReadCharacter();
            var nextPeekedCharacter = reader.PeekCharacter();

            var tokenFactoryFound = TokenFactories.FirstOrDefault(x => x.IsToken(characterRead, nextPeekedCharacter)) ?? throw new Exception($"No Token Found For Value = {characterRead}{nextPeekedCharacter}");

            tokens.Add(tokenFactoryFound.CreateToken(characterRead, reader));
        }

        return tokens.ToImmutableList();
    }
}
