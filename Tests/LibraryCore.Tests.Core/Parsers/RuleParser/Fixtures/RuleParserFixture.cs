using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

public class RuleParserFixture
{
    public RuleParserFixture()
    {
        ServiceProvider = new ServiceCollection()
           .AddAndConfigureRuleParser()
               .WithRegisterMethod("MyMethod1", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerId))!)
               .WithRegisterMethod("GetAnswerArray", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerArray))!)
               .WithRegisterMethod("GetNullableIntArray", typeof(RuleParserFixture).GetMethod(nameof(GetNullableIntArray))!)
               .WithRegisterMethod("GetANumberWithNoParameters", typeof(RuleParserFixture).GetMethod(nameof(GetANumberWithNoParameters))!)
           .BuildRuleParser()

           .BuildServiceProvider();
    }

    private IServiceProvider ServiceProvider { get; }
    public RuleParserEngine ResolveRuleParserEngine() => ServiceProvider.GetRequiredService<RuleParserEngine>();

    public static int GetANumberWithNoParameters() => 24;
    public static string GetAnswerId(Survey surveyModel, int questionId) => surveyModel.Answers[questionId];
    public static IEnumerable<int> GetAnswerArray(Survey surveyModel) => new[] { 1, 2, 3 };
    public static IEnumerable<int?> GetNullableIntArray(Survey surveyModel) => new List<int?> { 1, 2, 3, 80 };
}
