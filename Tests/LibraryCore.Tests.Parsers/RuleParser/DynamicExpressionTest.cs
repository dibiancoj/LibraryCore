using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.ComponentModel;
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
            },
            MyArrayOfInts = new[] { 1, 2, 3 }
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
                },
                MyArrayOfInts = SchemaDataType.Int
            }
        };

        var expressionToRun = RuleParserFixture.ResolveRuleParserEngine()
                                           .ParseString("$Survey.SurgeryCount$ == 26 || $Survey.SubObject.Id$ == '99'", schema)
                                           .BuildExpression<JsonElement>("Survey");

        Assert.Equal(expectedResult, expressionToRun.Compile().Invoke(JsonSerializer.Deserialize<dynamic>(makeDynamicObject)));
    }

    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    [Theory]
    public void IntContainsTest(int valueToTestContains, bool expectedResult)
    {
        var makeDynamicObject = JsonSerializer.Serialize(new
        {
            SurgeryCount = 5,
            Bla = false,
            Str = "sadfsd",
            Dt = DateTime.Now,
            SubObject = new
            {
                Id = "Test"
            },
            MyArrayOfInts = new[] { 1, 2, 3 }
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
                },
                MyArrayOfInts = SchemaDataType.ArrayOfInts
            }
        };

        var expressionToRun = RuleParserFixture.ResolveRuleParserEngine()
                                           .ParseString($"$Survey.MyArrayOfInts$ contains {valueToTestContains} || $Survey.SubObject.Id$ == '99'", schema)
                                           .BuildExpression<JsonElement>("Survey");

        Assert.Equal(expectedResult, expressionToRun.Compile().Invoke(JsonSerializer.Deserialize<dynamic>(makeDynamicObject)));
    }

    [InlineData("abc", true)]
    [InlineData("def", true)]
    [InlineData("xyz", false)]
    [InlineData("qrs", false)]
    [Theory]
    public void StringContainsTest(string valueToTestContains, bool expectedResult)
    {
        var makeDynamicObject = JsonSerializer.Serialize(new
        {
            SurgeryCount = 5,
            Bla = false,
            Str = "sadfsd",
            Dt = DateTime.Now,
            SubObject = new
            {
                Id = "Test"
            },
            MyArrayOfStrings = new[] { "abc", "def" }
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
                },
                MyArrayOfStrings = SchemaDataType.ArrayOfStrings
            }
        };

        var expressionToRun = RuleParserFixture.ResolveRuleParserEngine()
                                           .ParseString($"$Survey.MyArrayOfStrings$ contains '{valueToTestContains}' || $Survey.SubObject.Id$ == '99'", schema)
                                           .BuildExpression<JsonElement>("Survey");

        Assert.Equal(expectedResult, expressionToRun.Compile().Invoke(JsonSerializer.Deserialize<dynamic>(makeDynamicObject)));
    }
}
