using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Parsers.RuleParser.TokenFactories;
using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using System.Collections;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.Utilities;

public static class RuleParsingUtility
{
    internal const char NullableDataTypeIdentifier = '?';

    public record MethodParsingResult(string MethodName, IImmutableList<IToken> Parameters);

    internal static MethodParsingResult ParseMethodSignature(StringReader reader,
                                                             CreateTokenParameters createTokenParameters,
                                                             char closingCharacter = ')')
    {
        var methodName = WalkUntil(reader, '(', true);
        var text = new StringBuilder();

        //could have a func with a .Any(x => x.Upper() == 'Test')
        //start with 1 because its the opening bracket. Then when we hit the opening bracket we should be at 2
        int openingBrackets = 1;

        //eat until the end of the method
        while (reader.HasMoreCharacters())
        {
            var characterRead = reader.ReadCharacter();

            if (characterRead == '(')
            {
                openingBrackets++;
            }
            else if (characterRead == ')')
            {
                openingBrackets--;
            }

            if (characterRead == closingCharacter && openingBrackets == 0)
            {
                break;
            }

            text.Append(characterRead);
        }

        //no parameters
        if (text.Length == 0)
        {
            return new MethodParsingResult(methodName, ImmutableList<IToken>.Empty);
        }

        var tokenList = new List<IToken>();

        foreach (var parameter in text.ToString().Split(','))
        {
            using var parameterReader = new StringReader(parameter.Trim());

            var characterRead = parameterReader.ReadCharacter();
            var nextPeekedCharacter = parameterReader.PeekCharacter();

            if (createTokenParameters.TokenFactoryProvider.ResolveSpecificFactory<LambdaFactory>().IsToken(characterRead, nextPeekedCharacter, parameter))
            {
                //is it a lamda
                tokenList.Add(createTokenParameters.TokenFactoryProvider.ResolveSpecificFactory<LambdaFactory>().CreateToken(characterRead, parameterReader, createTokenParameters));
            }
            else
            {
                var readAndPeaked = new string([characterRead, nextPeekedCharacter]);

                tokenList.Add(createTokenParameters.TokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter, readAndPeaked)
                                                  .CreateToken(characterRead, parameterReader, createTokenParameters));
            }
        }

        return new MethodParsingResult(methodName, tokenList.ToImmutableList());
    }

    internal static Type DetermineGenericType(Expression instance)
    {
        if (instance.Type.IsGenericType)
        {
            return instance.Type.GenericTypeArguments[0];
        }

        if (typeof(IEnumerable).IsAssignableFrom(instance.Type))
        {
            return instance.Type.GetElementType() ?? throw new Exception("GetElementType Is Null");
        }

        return instance.Type;
    }

    /// <summary>
    /// Walk the parmeters in a method or between (....). This is specifically for method parameter parsing but can be used. The reader should be passed in with the first character being '('
    /// Syntax (24,true,'test'). This will work with multiple scenarios
    /// </summary>
    internal static IEnumerable<IToken> WalkTheParameterString(StringReader reader,
                                                               char closingCharacter,
                                                               CreateTokenParameters createTokenParameters)
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
            var readAndPeaked = new string([characterRead, nextPeekedCharacter]);

            yield return createTokenParameters.TokenFactoryProvider.ResolveTokenFactory(characterRead, nextPeekedCharacter, readAndPeaked)
                                    .CreateToken(characterRead, parameterReader, createTokenParameters);
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
            EatOrThrowCharacters(reader, new string([characterToStop]));
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
