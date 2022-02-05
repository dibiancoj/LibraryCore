using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class ArrayFactory : ITokenFactory
{
    //[1,2,3]
    //['string 1','string 2', 'string 3]

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == '[';

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != ']')
        {
            //need to determine the method name so we walk 
            
            var parameterGroup = RuleParsingUtility.WalkTheParameterString(stringReader, tokenFactoryProvider, ']').ToArray();

            return new ArrayToken(parameterGroup);
        }

        throw new Exception("MethodCallFactory Not Able To Parse Information");
    }
}

[DebuggerDisplay("{Values}")]
public record ArrayToken(IEnumerable<IToken> Values) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters)
    {
        var type = Values.Any(x => x is NumberToken) ?
                        typeof(int) :
                        typeof(string);

        return Expression.NewArrayInit(type, Values.Select(x => x.CreateExpression(parameters)));
    }
}