using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class MethodCallFactory : ITokenFactory
{
    private Dictionary<string, MethodInfo> RegisterdMethods { get; } = new();

    public MethodCallFactory RegisterNewMethodAlias(string key, MethodInfo registeredMethodParameters)
    {
        RegisterdMethods.Add(key, registeredMethodParameters);
        return this;
    }

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == '@';

    //@MyMethod(1)
    //@MyMethod(1,'abc', true)

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
        {
            //need to determine the method name so we walk 
            var methodName = RuleParsingUtility.WalkUntil(stringReader, '(');

            if (!RegisterdMethods.TryGetValue(methodName, out var tryToGetMethodInfoResult))
            {
                throw new Exception($"Method Name = {methodName} Is Not Registered In MethodCallFactory. Call {nameof(RegisterNewMethodAlias)} To Register The Method");
            }

            //eat the opening (
            RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, '(');

            bool hasNoParameters = stringReader.PeekCharacter() == ')';

            var parameterGroup = hasNoParameters ?
                                        Enumerable.Empty<IToken>() :
                                        RuleParsingUtility.WalkTheParameterString(stringReader, tokenFactoryProvider, ')', ruleParserEngine).ToArray();

            if (hasNoParameters)
            {
                //closing )
                RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, ')');
            }

            return new MethodCallToken(tryToGetMethodInfoResult, parameterGroup);
        }

        throw new Exception("MethodCallFactory Not Able To Parse Information");
    }
}

[DebuggerDisplay("Method Call {RegisteredMethodToUse}")]
public record MethodCallToken(MethodInfo RegisteredMethodToUse, IEnumerable<IToken> AdditionalParameters) : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters)
    {
        //convert all the additional parameters to an expression
        var parameterExpression = AdditionalParameters.Select(x => x.CreateExpression(parameters)).ToArray();

        return Expression.Call(RegisteredMethodToUse, parameterExpression);
    }
}