using LibraryCore.Core.Parsers.RuleParser;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserExpressionBuilderTest : IClassFixture<RuleParserFixture>
{
    public record SurveyModel(string Name, int Age, bool IsMarried, IDictionary<int, string> Answers);
    private SurveyModel Model { get; }
    private RuleParserFixture RuleParserFixture { get; }

    public RuleParserExpressionBuilderTest(RuleParserFixture ruleParserFixture)
    {
        Model = new SurveyModel("Jacob DeGrom", 30, true, new Dictionary<int, string>
        {
            { 1, "Yes" },
            { 2, "High" }
        });
        RuleParserFixture = ruleParserFixture;
    }

    #region Basic Token Type Calls

    [Fact]
    public void PropertyNamePositiveRule()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void PropertyNameWithOneParameterWhichIsNotSpecifiedPositiveRule()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Age == 30");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void NonObjectParameter()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Size == 25");
        var expression = RuleParserExpressionBuilder.BuildExpression<int>(tokens, "Size");

        Assert.True(expression.Compile().Invoke(25));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void PropertyNamePositiveRuleWithBoolean(bool isMarriedFlag)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString($"$Survey.IsMarried == {isMarriedFlag}");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(isMarriedFlag, expression.Compile().Invoke(Model));
    }

    [Fact]
    public void PropertyNameNegativeRule()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 29");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.False(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void PropertyNameRuleWithAndStatementPositive()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 && $Survey.Name == 'Jacob DeGrom'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void PropertyNameRuleWithAndStatementNegative()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 && $Survey.Name == 'John DeGrom'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.False(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void PropertyNameRuleWithAndStatementNegativeButHasOrStatement()
    {
        //Or statement makes this positive
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 || $Survey.Name == 'John DeGrom'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void BasicMethodCallPositive()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1(1) == 'Yes'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void BasicMethodCallNegative()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1(1) == 'No'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.False(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void MultipleMethodCallWithPropertyCall()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1(1) == 'Yes' && @MyMethod1(2) == 'High' && $Survey.Age > 5");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void MultipleMethodCallWithPropertyCallShouldFail()
    {
        //should fail because MyMethod(2) <> 'High'
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1(1) == 'Yes' && @MyMethod1(2) == 'Low' && $Survey.Age > 5");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.False(expression.Compile().Invoke(Model));
    }

    #endregion

    #region Comparison Tests

    //age is 30 for these tests

    //equals
    [InlineData("$Survey.Age == 30",true)]
    [InlineData("$Survey.Age == 29", false)]

    //not equal
    [InlineData("$Survey.Age != 29", true)]
    [InlineData("$Survey.Age != 30", false)]
    [InlineData("$Survey.Age != 31", true)]

    //less then
    [InlineData("$Survey.Age < 31", true)]
    [InlineData("$Survey.Age < 30", false)]

    //less then or equal
    [InlineData("$Survey.Age <= 30", true)]
    [InlineData("$Survey.Age <= 31", true)]
    [InlineData("$Survey.Age <= 29", false)]

    //greater then
    [InlineData("$Survey.Age > 29", true)]
    [InlineData("$Survey.Age > 30", false)]
    [InlineData("$Survey.Age > 31", false)]

    //greater then or equal
    [InlineData("$Survey.Age >= 29", true)]
    [InlineData("$Survey.Age >= 30", true)]
    [InlineData("$Survey.Age >= 31", false)]
    [Theory]
    public void ComparisonTest(string statementToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(statementToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(Model));
    }

    #endregion

    #region Combiner Tests [AndAlso OrElse]

    [Fact]
    public void AndAlsoIsTrue()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 && $Survey.IsMarried == True");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void AndAlsoIsFalse()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 && $Survey.IsMarried != True");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.False(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void OrElseIsTrueWhenBothAreTrue()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 || $Survey.IsMarried == True");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void OrElseIsTrueWhenFirstClauseIsTrue()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 || $Survey.IsMarried != True");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void OrElseIsTrueWhenSecondClauseIsTrue()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 29 || $Survey.IsMarried == True");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void OrElseIsFalse()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 29 || $Survey.IsMarried != True");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.False(expression.Compile().Invoke(Model));
    }

    [Fact]
    public void OrElseWith3Clauses()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 29 || $Survey.IsMarried != True || @MyMethod1(2) == 'High'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(Model));
    }

    #endregion

    #region Linq Statements

    [Fact]
    public void LinqWithExpression()
    {
        var dataSet = new List<SurveyModel>
        {
            new SurveyModel("John", 21, false, new Dictionary<int,string>{ {1, "NA" } }),
            new SurveyModel("Jacob", 30, true, new Dictionary<int,string>{ {1, "High" } }),
            new SurveyModel("Jason", 37, true, new Dictionary<int,string>{ {1, "Low" } })
        };

        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Name == 'Jason'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        var filteredDataSet = dataSet.AsQueryable().Where(expression).ToList();

        Assert.Single(filteredDataSet);
        Assert.Equal("Jason", filteredDataSet.Single().Name);
    }

    [Fact]
    public void LinqWithExpressionWithAndStatement()
    {
        var dataSet = new List<SurveyModel>
        {
            new SurveyModel("John", 21, false, new Dictionary<int,string>{ {1, "NA" } }),
            new SurveyModel("Jacob", 30, true, new Dictionary<int,string>{ {1, "High" } }),
            new SurveyModel("Jason", 37, true, new Dictionary<int,string>{ {1, "Low" } })
        };

        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Name == 'John' && $Survey.IsMarried != True");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        var filteredDataSet = dataSet.AsQueryable().Where(expression).ToList();

        Assert.Single(filteredDataSet);
        Assert.Equal("John", filteredDataSet.Single().Name);
    }

    [Fact]
    public void LinqWithExpressionWithMethodCall()
    {
        var dataSet = new List<SurveyModel>
        {
            new SurveyModel("John", 21, false, new Dictionary<int,string>{ {1, "NA" } }),
            new SurveyModel("Jacob", 30, true, new Dictionary<int,string>{ {1, "High" } }),
            new SurveyModel("Jason", 37, true, new Dictionary<int,string>{ {1, "Low" } })
        };

        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1(1) == 'High'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        var filteredDataSet = dataSet.AsQueryable().Where(expression).ToList();

        Assert.Single(filteredDataSet);
        Assert.Equal("Jacob", filteredDataSet.Single().Name);
    }

    #endregion

    #region Multiple Parameters

    [Fact]
    public void TwoParameterTest()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 && $Size.Age == 12");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel, SurveyModel>(tokens, "Survey", "Size");

        Assert.True(expression.Compile().Invoke(Model, new SurveyModel("Jacob", 12, false, new Dictionary<int, string>())));
    }

    [Fact]
    public void ThreeParameterTest()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 && $Size.Age == 12 && $Color.Age == 15");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel, SurveyModel, SurveyModel>(tokens, "Survey", "Size", "Color");

        Assert.True(expression.Compile().Invoke(Model,
                                                new SurveyModel("Jacob", 12, false, new Dictionary<int, string>()),
                                                new SurveyModel("Teenager", 15, false, new Dictionary<int, string>())));
    }

    #endregion

}
