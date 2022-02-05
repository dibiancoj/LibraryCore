using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using static LibraryCore.Tests.Core.Parsers.RuleParser.RuleParserExpressionBuilderTest;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserFixture
{
    public RuleParserFixture()
    {
        RuleParserEngineToUse = new RuleParserEngine(new TokenFactoryProvider(new ITokenFactory[]
        {
            new TrueFactory(),
            new FalseFactory(),
            new NullTokenFactory(),

            new ParameterPropertyFactory(),
            new WhiteSpaceFactory(),
            new StringFactory(),
            new NumberFactory(),
            new ArrayFactory(),
            new MethodCallFactory()
                   .RegisterNewMethodAlias("MyMethod1", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerId))!)
                   .RegisterNewMethodAlias("GetAnswerArray", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerArray))!)
                   .RegisterNewMethodAlias("GetANumberWithNoParameters", typeof(RuleParserFixture).GetMethod(nameof(GetANumberWithNoParameters))!),

            new LessThenOrEqualFactory(),
            new LessThenFactory(),
            new GreaterThenOrEqualFactory(),
            new GreaterThenFactory(),
            new EqualsFactory(),
            new NotEqualsFactory(),
            new ContainsFactory(),

            new OrElseFactory(),
            new AndAlsoFactory()
        }));
    }

    public RuleParserEngine RuleParserEngineToUse { get; }

    public static int GetANumberWithNoParameters() => 24;
    public static string GetAnswerId(SurveyModel surveyModel, int questionId) => surveyModel.Answers[questionId];
    public static IEnumerable<int> GetAnswerArray(SurveyModel surveyModel) => new[] { 1, 2, 3 };
}
