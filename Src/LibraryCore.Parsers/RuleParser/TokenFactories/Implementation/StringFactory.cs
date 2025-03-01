﻿using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class StringFactory : ITokenFactory
{
    /// <summary>
    /// Single quote
    /// </summary>
    private const char TokenIdentifier = '\'';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == TokenIdentifier;

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        var text = new StringBuilder();
        int numberOfSubTokens = 0;
        var subTokens = new List<IToken>();

        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != TokenIdentifier)
        {
            var currentCharacter = stringReader.ReadCharacter();

            //to support logging we are going to allow formatters in a string. ie: 'MedicationId = {$Parameter.MedicationId}'
            if (currentCharacter == '{')
            {
                subTokens.Add(ResolveInnerTokens(stringReader, createTokenParameters));

                //turn into a format syntax..{0}, {1}
                text.Append('{').Append(numberOfSubTokens).Append('}');

                numberOfSubTokens++;
            }
            else
            {
                text.Append(currentCharacter);
            }
        }

        //did we ever find a closing bracket?
        if (!stringReader.HasMoreCharacters())
        {
            throw new Exception("Missing closing quote on String Value. Current Value = " + text.ToString());
        }

        //read the closing '
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, TokenIdentifier);

        return new StringToken(text.ToString(), subTokens);
    }

    private static IToken ResolveInnerTokens(StringReader stringReader,
                                             CreateTokenParameters createTokenParameters)
    {
        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != '}')
        {
            var characterRead = stringReader.ReadCharacter();
            var characterPeeked = stringReader.PeekCharacter();

            var factoryFound = createTokenParameters.TokenFactoryProvider.ResolveTokenFactory(characterRead, characterPeeked, new string([characterRead, characterPeeked]));

            var token = factoryFound.CreateToken(characterRead, stringReader, createTokenParameters);

            //kill the closing }
            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, '}');

            return token;
        }

        string errorMessage = !stringReader.HasMoreCharacters() ?
                            "String Interpolation - No more characters to read after starting {" :
                            "String Interpolation - No value found inside interpolation - {}";

        throw new Exception(errorMessage);
    }

}

[DebuggerDisplay("{Value}")]
public record StringToken(string Value, IEnumerable<IToken> InnerTokens) : IToken
{
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters)
    {
        return InnerTokens.IsNullOrEmpty() ?
            Expression.Constant(Value) :
            CreateExpressionWithInnerFormats(parameters);
    }

    private MethodCallExpression CreateExpressionWithInnerFormats(IReadOnlyList<ParameterExpression> parameters)
    {
        //create an object[] with the correct number of parameters. ie: {0}  {1}...we would end up with object[1]
        var stringFormatObjectTypes = Enumerable.Range(0, InnerTokens.Count()).Select(x => typeof(object)).ToArray();

        //grab the format with the correct number of items
        var stringFormat = typeof(string).GetMethod(nameof(string.Format), [typeof(string), .. stringFormatObjectTypes]) ?? throw new Exception("Can't Find String.Format Method Info");

        //create all the parameters
        IEnumerable<Expression> tokens = InnerTokens.Select(x => Expression.Convert(x.CreateExpression(parameters), typeof(object)));

        //return the string.format call
        return Expression.Call(stringFormat, tokens.Prepend(Expression.Constant(Value)));
    }
}