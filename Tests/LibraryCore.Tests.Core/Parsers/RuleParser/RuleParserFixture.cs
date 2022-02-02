using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserFixture
{
    public RuleParserFixture()
    {
        RuleParserEngineToUse = new RuleParserEngine(new ITokenFactory[]
        {
            new TrueFactory(),
            new FalseFactory(),

            new WhiteSpaceFactory(),
            new StringFactory(),
            new NumberFactory(),

            new LessThenOrEqualFactory(),
            new LessThenFactory(),
            new GreaterThenOrEqualFactory(),
            new GreaterThenFactory(),
            new EqualsFactory(),

            new OrElseFactory(),
            new AndAlsoFactory()
        });
    }

    public RuleParserEngine RuleParserEngineToUse { get; }
}
