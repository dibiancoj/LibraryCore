using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using System.Collections.Immutable;

namespace LibraryCore.Core.Parsers.RuleParser;

public class RuleParserEngine
{
    public RuleParserEngine(TokenFactoryProvider tokenFactoryProvider)
    {
        TokenFactoryProvider = tokenFactoryProvider;
    }

    private TokenFactoryProvider TokenFactoryProvider { get; }

    //$ParameterName.PropertyName Of a property passed in
    //@MethodCall(1,true, 'sometext') <-- need to register the method in MethodCallFactory.RegisterNewMethodAlias. That says "MethodCall" goes to this method in this namespace
    //[1,2,3] <-- array of ints
    //['item 1', 'item 2', 'item 3'] <-- array of strings

    //Types:
    //SomeValue == 'some text'
    //SomeValue == true
    //SomeValue == 24
    //SomeValue == null

    //Comparison
    //== Equal
    //!= Does Not Equal
    //>
    //>=
    //<
    //<=
    //contains ie: [1,2,3] contains $Parameter.Age or @MethodWithArray contains $Parameter.Age

    //Combiners
    //&& AndAlso
    //|| OrElse

    public IImmutableList<Token> ParseString(string stringToParse)
    {
        using var reader = new StringReader(stringToParse);
        var tokens = new List<Token>();

        while (reader.HasMoreCharacters())
        {
            var characterRead = reader.ReadCharacter();
            var nextPeekedCharacter = reader.PeekCharacter();

            var tokenFactoryFound = TokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter);

            tokens.Add(tokenFactoryFound.CreateToken(characterRead, reader, TokenFactoryProvider));
        }

        return tokens.ToImmutableList();
    }
}
