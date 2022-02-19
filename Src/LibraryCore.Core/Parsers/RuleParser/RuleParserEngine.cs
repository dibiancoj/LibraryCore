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
    //Numbers:
    // 1     = 1 as an int
    // 1?    = 1 as nullable int
    // 1.5d  = 1.5 double
    // 1.5d? = 1.5 nullable double

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
    //like ie: 'tester' like 'test'

    //Combiners
    //&& AndAlso
    //|| OrElse

    public IImmutableList<IToken> ParseString(string stringToParse)
    {
        using var reader = new StringReader(stringToParse);
        var tokens = new List<IToken>();

        while (reader.HasMoreCharacters())
        {
            var characterRead = reader.ReadCharacter();
            var nextPeekedCharacter = reader.PeekCharacter();

            //alot of the tokens are looking for words or the first 2 characters. Combine it here so each rule doesn't need to create a string
            var readAndPeeked = new string(new[] { characterRead, nextPeekedCharacter });

            var tokenFactoryFound = TokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter, readAndPeeked);

            tokens.Add(tokenFactoryFound.CreateToken(characterRead, reader, TokenFactoryProvider));
        }

        return tokens.ToImmutableList();
    }
}
