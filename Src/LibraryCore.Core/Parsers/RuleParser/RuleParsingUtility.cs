using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser;

public static class RuleParsingUtility
{
    /// <summary>
    /// Walk the parmeters in a method or between (....). This is specifically for method parameter parsing but can be used. The reader should be passed in with the first character being '('
    /// Syntax (24,true,'test'). This will work with multiple scenarios
    /// </summary>
    public static IEnumerable<Token> WalkTheParameterString(StringReader reader, TokenFactoryProvider tokenFactoryProvider, char closingCharacter)
    {
        var text = new StringBuilder();

        //eat the opening (
        _ = reader.Read();

        //eat until the end of the method
        while (reader.HasMoreCharacters() && reader.PeekCharacter() != closingCharacter)
        {
            text.Append(reader.ReadCharacter());
        }

        //eat the closing )
        _ = reader.Read();

        foreach (var parameter in text.ToString().Split(','))
        {
            using var parameterReader = new StringReader(parameter.Trim());

            var characterRead = parameterReader.ReadCharacter();
            var nextPeekedCharacter = parameterReader.PeekCharacter();

            yield return tokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter).CreateToken(characterRead, parameterReader, tokenFactoryProvider);
        }
    }
}
