using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Text.Json;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;
using static LibraryCore.Parsers.RuleParser.Utilities.SchemaModel;

namespace LibraryCore.Tests.Parsers.RuleParser;

public class ExpressionBuilderTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public ExpressionBuilderTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [Fact]
    public void NoParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2")
                                                .BuildExpression();

        Assert.False(expression.Compile().Invoke());
    }

    [Fact]
    public void NoParameterTestFromCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2")
                                                .BuildExpression()
                                                .Compile();

        Assert.False(expression.Invoke());
    }

    [InlineData(5, false)]
    [InlineData(30, true)]
    [Theory]
    public void SimpleParameterTest(int age, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Age$ > 15")
                                                .BuildExpression<int>("Age")
                                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(age));
    }

    [Fact]
    public void TwoParameterTest()
    {
        //var expression = RuleParserFixture.ResolveRuleParserEngine()
        //                                    .ParseString("$Survey.SurgeryCount$ == 25 || $Size.Abc$ == 'test12' || $Survey.Bla$ == true")
        //                                    .BuildExpression<dynamic, dynamic>("Survey", "Size");



        //var exp1 = new ExpandoObject();
        //exp1.TryAdd("SurgeryCount", 24);
        //exp1.TryAdd("Bla", true);

        //var exp2 = new ExpandoObject();
        //exp2.TryAdd("Abc", "test");

        //var result = expression.Compile().Invoke(exp1, exp2);

        //Assert.True(result);

        //////////////////
        //json element now
        var jsonString = JsonSerializer.Serialize(new
        {
            SurgeryCount = 25,
            Bla = false,
            Str = "sadfsd",
            Dt = DateTime.Now,
            SubObject = new
            {
                Id = "99"
            }
        });

        var dynamicFromJsonElement = JsonSerializer.Deserialize<dynamic>(jsonString);

        var expression2 = RuleParserFixture.ResolveRuleParserEngine()
                                           .ParseString("$Survey.SurgeryCount$ == 26 || $Survey.SubObject.Id$ == '99'",
                                           () => new
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
                                           })
                                           .BuildExpression<JsonElement>("Survey");

        var result2 = expression2.Compile().Invoke(dynamicFromJsonElement);

        Assert.True(result2);
    }

    [Fact]
    public void TwoParameterTestWithCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 24 && $Size.SurgeryCount$ == 12")
                                            .BuildExpression<Survey, Survey>("Survey", "Size")
                                            .Compile();

        Assert.True(expression.Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(24)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value));
    }

    [Fact]
    public void ThreeParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
                                            .BuildExpression<Survey, Survey, Survey>("Survey", "Size", "Color");

        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(30)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(15)
                                                .Value));
    }

    [Fact]
    public void ThreeParameterTestWithCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
                                            .BuildExpression<Survey, Survey, Survey>("Survey", "Size", "Color")
                                            .Compile();

        Assert.True(expression.Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(30)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(15)
                                                .Value));
    }

}
