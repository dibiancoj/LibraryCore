using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Parsers.RuleParser.TokenFactories;
using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;

namespace LibraryCore.Parsers.RuleParser;

public class RuleParserEngine(TokenFactoryProvider tokenFactoryProvider)
{

    //Types:
    //SomeValue == 'some text'          <-- string --> You can also add formatter tags in a string because of the logger code. ie: 'Medication Id = {$Request.MedicationId$}
    //SomeValue == true                 <-- boolean
    //SomeValue == 24                   <-- int
    //SomeValue == 24?                  <-- nullable int
    //SomeValue == 24d                  <-- double
    //SomeValue == 24d?                 <-- nullable double
    //SomeValue == null                 <-- null values
    //SomeValue == ^1/1/2000^           <-- date
    //SomeValue == ^1/1/2022 1:00pm^    <-- date with time
    //SomeValue == ^1/1/2000^?          <-- nullable date
    //SomeValue == ^1/1/2000 1:00pm^?   <-- nullable date with time
    //[1,2,3] contains 2                <-- array of ints
    //['a1','a2'] contains 'a2'         <-- array of strings
    //SomeValue == #EnumTypeFullNamespace|Male#        <-- Enum Value
    //SomeValue == #EnumTypeFullNamespace|Male?#        <-- Enum Value With Nullable Comparison

    //Parameter / Methods Calls
    //$ParameterName.PropertyName$ Of a property passed in
    //$MyBooleanParameter$ if the parameter is not an object. ie: $MyBooleanParameter$ == true
    //@MethodCall(1,true, 'sometext') <-- need to register the method in MethodCallFactory.RegisterNewMethodAlias. That says "MethodCall" goes to this method in this namespace

    //instance method calls and linq (you can chain the calls together too)
    //$myString$.ToUpper() == 'HIGH'
    //[1,2,3].Any($x$ => $x$ > 3) == true
    //"$Surveys$.Any($x$ => $x.Name$ == 'Test') == true"
    //@MethodCallToGetArray().Count($x$ => $x$ == 3) > 2

    //Comparison
    //== Equal
    //!= Does Not Equal
    //>
    //>=
    //<
    //<=
    //contains ie: [1,2,3] contains $Parameter.Age$ or @MethodWithArray contains $Parameter.Age
    //like ie: 'tester' like 'test'

    //Combiners
    //&& AndAlso
    //|| OrElse

    public record CreateTokenParameters(TokenFactoryProvider TokenFactoryProvider, RuleParserEngine RuleParserEngine, SchemaModel SchemaConfiguration);

    public RuleParserCompilationResult ParseString(string stringToParse, object? schemaModel = null)
    {
        using var reader = new StringReader(stringToParse);
        var tokens = new List<IToken>();
        var schema = SchemaModel.Create(schemaModel);
        var createTokenParameters = new CreateTokenParameters(tokenFactoryProvider, this, schema);

        while (reader.HasMoreCharacters())
        {
            var characterRead = reader.ReadCharacter();
            var nextPeekedCharacter = reader.PeekCharacter();

            //alot of the tokens are looking for words or the first 2 characters. Combine it here so each rule doesn't need to create a string
            var readAndPeeked = new string([characterRead, nextPeekedCharacter]);

            var tokenFactoryFound = tokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter, readAndPeeked);

            tokens.Add(tokenFactoryFound.CreateToken(characterRead, reader, createTokenParameters));
        }

        return new RuleParserCompilationResult(tokens.ToImmutableList());
    }

    public RuleParserCompilationResult<TScoreType> ParseScore<TScoreType>(params ScoringCriteriaParameter<TScoreType>[] scoreCriteria)
    {
        return new RuleParserCompilationResult<TScoreType>(
            scoreCriteria.Select(t => (IToken)new ScoreCriteriaToken<TScoreType>(t.ScoreValueIfTrue, ParseString(t.ScoreTruthCriteria).CompilationTokenResult)).ToImmutableList(), SchemaModel.Create(null));
    }
}
