﻿using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser;

public static class RuleParsingUtility
{
    /// <summary>
    /// Walk the parmeters in a method or between (....). This is specifically for method parameter parsing but can be used. The reader should be passed in with the first character being '('
    /// Syntax (24,true,'test'). This will work with multiple scenarios
    /// </summary>
    public static IEnumerable<IToken> WalkTheParameterString(StringReader reader, TokenFactoryProvider tokenFactoryProvider, char closingCharacter)
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

            yield return tokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter, readAndPeaked).CreateToken(characterRead, parameterReader, tokenFactoryProvider);
        }
    }

    public static void ThrowIfCharacterNotExpected(StringReader reader, params char[] expectedCharacterRead)
    {
        char characterRead = reader.ReadCharacter();

        if (!expectedCharacterRead.Contains(characterRead))
        {
            throw new Exception($"Character Read {characterRead} Is Not Expected. Expected Character = {string.Join(" or ",expectedCharacterRead)}");
        }
    }
}
