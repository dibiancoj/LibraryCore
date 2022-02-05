using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class ContainsFactory : ITokenFactory
{
    //[1,2,3] contains 2

    private ContainsToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "co", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the other c...ontains
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'O', 'o');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'N', 'n');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'T', 't');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'A', 'a');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'I', 'i');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'N', 'n');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'S', 's');

        return CachedToken;
    }
}

[DebuggerDisplay("in")]
public record ContainsToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateBinaryOperatorExpression(Expression left, Expression right)
    {
        var containsMethodInfo = CreateEnumerableContains(left);

        return Expression.Call(null, containsMethodInfo, left, right);
    }

    private static MethodInfo CreateEnumerableContains(Expression left)
    {
        Type typeOfArray = typeof(IEnumerable<int>).IsAssignableFrom(left.Type) ?
                                        typeof(int) :
                                        typeof(string);

        return typeof(Enumerable).GetMethods()
                                .Where(x => x.Name == nameof(Enumerable.Contains) && x.IsGenericMethod && x.GetParameters().Length == 2)
                                .Single()
                                .MakeGenericMethod(typeOfArray);
    }

    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();
}
