using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class StringFactory : ITokenFactory
{
    /// <summary>
    /// Single quote
    /// </summary>
    private const char TokenIdentifier = '\'';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == TokenIdentifier;

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != TokenIdentifier)
        {
            text.Append(stringReader.ReadCharacter());
        }

        //did we ever find a closing bracket?
        if (!stringReader.HasMoreCharacters())
        {
            throw new Exception("Missing closing quote on String Value. Current Value = " + text.ToString());
        }

        //read the closing '
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, TokenIdentifier);

        var textToString = text.ToString();

        var innerTokens = new List<IToken>();

        //do we have inner expression?
        if (textToString.Contains('{'))
        {
            ///********** NEED TO LOOP THROUGH THE {....}

            var toSpan = textToString.AsSpan();

            var sliced = toSpan.Slice(toSpan.IndexOf('{') + 1, toSpan.IndexOf('}') - toSpan.IndexOf('{') - 1);

            var innerFactory = tokenFactoryProvider.ResolveTokenFactory(sliced[0], sliced[1], new string(sliced.Slice(1, 2)));

            innerTokens.Add(innerFactory.CreateToken(sliced[0], new StringReader(new string(sliced)), tokenFactoryProvider));
        }

        return new StringToken(textToString, innerTokens);
    }

}

[DebuggerDisplay("{Value}")]
public record StringToken(string Value, IEnumerable<IToken> InnerTokens) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters)
    {
        var t = InnerTokens.First().CreateExpression(parameters);

        var stringConcat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });

        ParameterExpression variableExpr = Expression.Variable(typeof(int), "sampleVar");

        Expression assignExpr = Expression.Assign(
            variableExpr,
            t
            );

        //return Expression.Call(stringConcat!, Expression.Constant(Value), Expression.Call(toString!, t));

        return Expression.Constant(Value);
    }
}