using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.Utilities;

public static class RuleParsingUtility
{
    internal const char NullableDataTypeIdentifier = '?';

    public record MethodParsingResult(string MethodName, IEnumerable<IToken> Parameters);

    internal static MethodParsingResult ParseMethodSignature(StringReader reader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine, char closingCharacter = ')')
    {
        var methodName = WalkUntil(reader, '(');
        var text = new StringBuilder();

        //eat until the end of the method
        while (reader.HasMoreCharacters() && reader.PeekCharacter() != closingCharacter)
        {
            text.Append(reader.ReadCharacter());
        }

        //eat the closing )
        ThrowIfCharacterNotExpected(reader, closingCharacter);

        //no parameters
        if (text.Length == 1)
        {
            return new MethodParsingResult(methodName, Array.Empty<IToken>());
        }

        var tokenList = new List<IToken>();

        foreach (var parameter in text.ToString().Split(','))
        {
            using var parameterReader = new StringReader(parameter.Trim());

            var characterRead = parameterReader.ReadCharacter();
            var nextPeekedCharacter = parameterReader.PeekCharacter();

            if (tokenFactoryProvider.ResolveSpecificFactory<LambdaFactory>().IsToken(characterRead, nextPeekedCharacter, parameter))
            {
                //is it a lamda
                tokenList.Add(tokenFactoryProvider.ResolveSpecificFactory<LambdaFactory>().CreateToken(characterRead, parameterReader, tokenFactoryProvider, ruleParserEngine));
            }
            else
            {
                var readAndPeaked = new string(new[] { characterRead, nextPeekedCharacter });

                tokenList.Add(tokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter, readAndPeaked).CreateToken(characterRead, parameterReader, tokenFactoryProvider, ruleParserEngine));
            }
        }

        return new MethodParsingResult(methodName, tokenList);
    }

    /// <summary>
    /// Walk the parmeters in a method or between (....). This is specifically for method parameter parsing but can be used. The reader should be passed in with the first character being '('
    /// Syntax (24,true,'test'). This will work with multiple scenarios
    /// </summary>
    internal static IEnumerable<IToken> WalkTheParameterString(StringReader reader, TokenFactoryProvider tokenFactoryProvider, char closingCharacter, RuleParserEngine ruleParserEngine)
    {
        var text = new StringBuilder();

        //eat until the end of the method
        while (reader.HasMoreCharacters() && reader.PeekCharacter() != closingCharacter)
        {
            text.Append(reader.ReadCharacter());
        }

        //eat the closing )
        ThrowIfCharacterNotExpected(reader, closingCharacter);

        foreach (var parameter in text.ToString().Split(','))
        {
            using var parameterReader = new StringReader(parameter.Trim());

            var characterRead = parameterReader.ReadCharacter();
            var nextPeekedCharacter = parameterReader.PeekCharacter();
            var readAndPeaked = new string(new[] { characterRead, nextPeekedCharacter });

            yield return tokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter, readAndPeaked).CreateToken(characterRead, parameterReader, tokenFactoryProvider, ruleParserEngine);
        }
    }

    internal static string WalkUntil(StringReader reader, char characterToStop, bool eatCharacterToStop = false)
    {
        var text = new StringBuilder();

        while (reader.HasMoreCharacters() && reader.PeekCharacter() != characterToStop)
        {
            text.Append(reader.ReadCharacter());
        }

        if (eatCharacterToStop)
        {
            EatOrThrowCharacters(reader, new string(new[] { characterToStop }));
        }

        return text.ToString();
    }

    internal static void ThrowIfCharacterNotExpected(StringReader reader, params char[] expectedCharacterRead)
    {
        char characterRead = reader.ReadCharacter();

        if (!expectedCharacterRead.Contains(characterRead))
        {
            throw new Exception($"Character Read {characterRead} Is Not Expected. Expected Character = {string.Join(" or ", expectedCharacterRead)}");
        }
    }

    internal static string WalkUntilEof(StringReader reader)
    {
        var text = new StringBuilder();

        while (reader.HasMoreCharacters())
        {
            text.Append(reader.ReadCharacter());
        }

        return text.ToString();
    }

    internal static Type DetermineNullableType<T, TNullableType>(StringReader reader)
    {
        if (reader.PeekCharacter() == NullableDataTypeIdentifier)
        {
            ThrowIfCharacterNotExpected(reader, NullableDataTypeIdentifier);
            return typeof(TNullableType);
        }

        return typeof(T);
    }

    internal static void EatOrThrowCharacters(StringReader stringReader, string charactersToEat)
    {
        foreach (var characterToEat in charactersToEat)
        {
            ThrowIfCharacterNotExpected(stringReader, char.ToUpper(characterToEat), char.ToLower(characterToEat));
        }
    }
}
