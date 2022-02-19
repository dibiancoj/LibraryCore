using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

public class RuleParserFixture
{
    public RuleParserFixture()
    {
        RuleParserEngineToUse = new RuleParserEngine(new TokenFactoryProvider(new ITokenFactory[]
        {
            new BooleanFactory(),
            new NullTokenFactory(),

            new ParameterPropertyFactory(),
            new WhiteSpaceFactory(),
            new StringFactory(),
            new NumberFactory(),
            new DateFactory(),
            new ArrayFactory(),
            new MethodCallFactory()
                   .RegisterNewMethodAlias("MyMethod1", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerId))!)
                   .RegisterNewMethodAlias("GetAnswerArray", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerArray))!)
                   .RegisterNewMethodAlias("GetNullableIntArray", typeof(RuleParserFixture).GetMethod(nameof(GetNullableIntArray))!)
                   .RegisterNewMethodAlias("GetANumberWithNoParameters", typeof(RuleParserFixture).GetMethod(nameof(GetANumberWithNoParameters))!),

            new LessThenOrEqualFactory(),
            new LessThenFactory(),
            new GreaterThenOrEqualFactory(),
            new GreaterThenFactory(),
            new EqualsFactory(),
            new NotEqualsFactory(),
            new ContainsFactory(),
            new LikeFactory(),

            new OrElseFactory(),
            new AndAlsoFactory()
        }));
    }

    public RuleParserEngine RuleParserEngineToUse { get; }

    public static int GetANumberWithNoParameters() => 24;
    public static string GetAnswerId(Survey surveyModel, int questionId) => surveyModel.Answers[questionId];
    public static IEnumerable<int> GetAnswerArray(Survey surveyModel) => new[] { 1, 2, 3 };
    public static IEnumerable<int?> GetNullableIntArray(Survey surveyModel) => new List<int?> { 1, 2, 3, 80 };
}
