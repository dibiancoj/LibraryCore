using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

public class RuleParserFixture
{
    public RuleParserFixture()
    {
        ServiceProvider = new ServiceCollection()
           .AddRuleParserWithConfiguration()
               .WithRegisterMethod("MyMethod1", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerId))!)
               .WithRegisterMethod("GetAnswerArray", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerArray))!)
               .WithRegisterMethod("GetNullableIntArray", typeof(RuleParserFixture).GetMethod(nameof(GetNullableIntArray))!)
               .WithRegisterMethod("GetANumberWithNoParameters", typeof(RuleParserFixture).GetMethod(nameof(GetANumberWithNoParameters))!)
               .WithRegisterMethod("ExtractFromDictionary", typeof(RuleParserFixture).GetMethod(nameof(ExtractFromDictionary))!)
           .BuildRuleParser()

           .BuildServiceProvider();
    }

    private IServiceProvider ServiceProvider { get; }
    public RuleParserEngine ResolveRuleParserEngine() => ServiceProvider.GetRequiredService<RuleParserEngine>();

    public static int GetANumberWithNoParameters() => 24;
    public static string GetAnswerId(Survey surveyModel, int questionId) => surveyModel.Answers[questionId];
    public static IEnumerable<int> GetAnswerArray(Survey surveyModel) => new[] { 1, 2, 3 };
    public static IEnumerable<int?> GetNullableIntArray(Survey surveyModel) => new List<int?> { 1, 2, 3, 80 };
    public static object ExtractFromDictionary(IDictionary<string, object> parameters, string nodeStatment)
    {
        //this would be the "saveRequest.DrugNameId"...split by property value
        var nodePropertiesToParse = nodeStatment.Split('.').AsSpan();

        //will always have atleast 1 record (even if we don't have any items that were split)
        if (!parameters.TryGetValue(nodePropertiesToParse[0], out var tempObject))
        {
            throw new Exception("Object Not Found In Parameter Path. Property Path = " + nodePropertiesToParse[0]);
        }

        //loop through all the property levels
        foreach (var propertyLevel in nodePropertiesToParse[1..])
        {
            //grab the property and the value
            tempObject = tempObject?.GetType().GetProperty(propertyLevel)?.GetValue(tempObject, null);
        }

        return tempObject;
    }
}
