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
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "ONTAINS");

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

    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    private static MethodInfo EnumerableContains => typeof(Enumerable).GetMethods()
                                                    .Where(x => x.Name == nameof(Enumerable.Contains) && x.IsGenericMethod && x.GetParameters().Length == 2)
                                                    .Single();

    private static MethodInfo CreateEnumerableContainsMethodInfo(Type typeOfArrayData) => EnumerableContains.MakeGenericMethod(typeOfArrayData);

    private static MethodInfo CreateEnumerableContains(Expression left)
    {
        //this might be really limited. This handles a method which returns the type. Really anything that is IEnumerable<T>
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(left.Type) && left.Type.IsGenericType)
        {
            return CreateEnumerableContainsMethodInfo(left.Type.GenericTypeArguments[0]);
        }

        return CreateEnumerableContainsMethodInfo(left.Type.GetElementType() ?? throw new Exception("Can't Get Element Type"));
    }
}
