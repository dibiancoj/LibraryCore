using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Text.Json;
using static LibraryCore.Parsers.RuleParser.Utilities.SchemaModel;

namespace LibraryCore.Tests.Parsers.RuleParser;

public class DynamicExpressionTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public DynamicExpressionTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [InlineData(26, true)]
    [InlineData(25, false)]
    [InlineData(24, false)]
    [Theory]
    public void SimpleParameterType(int surgeryCountValue, bool expectedResult)
    {
        var makeDynamicObject = JsonSerializer.Serialize(surgeryCountValue);

        var schema = new
        {
            SurgeryCount = SchemaDataType.Int
        };

        var expressionToRun = RuleParserFixture.ResolveRuleParserEngine()
                                           .ParseString("$SurgeryCount$ == 26", schema)
                                           .BuildExpression<JsonElement>("SurgeryCount");

        Assert.Equal(expectedResult, expressionToRun.Compile().Invoke(JsonSerializer.Deserialize<dynamic>(makeDynamicObject)));
    }

    [InlineData(26, "100", true)]
    [InlineData(25, "99", true)]
    [InlineData(25, "88", false)]
    [Theory]
    public void BasicTest(int surgeryCountValue, string subObjectIdValue, bool expectedResult)
    {
        var makeDynamicObject = JsonSerializer.Serialize(new
        {
            SurgeryCount = surgeryCountValue,
            Bla = false,
            Str = "sadfsd",
            Dt = DateTime.Now,
            SubObject = new
            {
                Id = subObjectIdValue
            }
        });

        var schema = new
        {
            Survey = new
            {
                SurgeryCount = SchemaDataType.Int,
                Bla = SchemaDataType.Boolean,
                Str = SchemaDataType.String,
                Dt = SchemaDataType.DateTime,
                SubObject = new
                {
                    Id = SchemaDataType.String
                }
            }
        };

        var z = JsonSerializer.SerializeToElement(new
        {
            SurgeryCount = surgeryCountValue,
            Bla = false,
            Str = "sadfsd",
            Dt = DateTime.Now,
            SubObject = new
            {
                Id = subObjectIdValue
            }
        });

        var expressionToRun = RuleParserFixture.ResolveRuleParserEngine()
                                           .ParseString("$Survey.SurgeryCount$ == 26 || $Survey.SubObject.Id$ == '99'", schema)
                                           .BuildExpression<JsonElement>("Survey");

        Assert.Equal(expectedResult, expressionToRun.Compile().Invoke(JsonSerializer.Deserialize<dynamic>(makeDynamicObject)));
    }
}
