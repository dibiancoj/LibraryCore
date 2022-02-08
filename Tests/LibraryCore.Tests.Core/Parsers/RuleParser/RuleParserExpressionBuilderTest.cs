using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserExpressionBuilderTest : IClassFixture<RuleParserFixture>
{
    public record SurveyModel(string Name, int Age, bool IsMarried, int? NumberOfKidsNullable, IDictionary<int, string> Answers, bool? NullableBoolTest = null, double? Price = null, double PriceNonNullable = 5);
    private SurveyModel Model { get; }
    private RuleParserFixture RuleParserFixture { get; }

    public RuleParserExpressionBuilderTest(RuleParserFixture ruleParserFixture)
    {
        Model = new SurveyModel("Jacob DeGrom", 30, true, null, new Dictionary<int, string>
        {
            { 1, "Yes" },
            { 2, "High" }
        }, Price: 5);
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
    public void BasicMethodCallWithNoParametersPositive()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("@GetANumberWithNoParameters() == 24");
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

    [InlineData("[1,2,3] contains $Survey.Age", false)]
    [InlineData("[1,2,3, 30] contains $Survey.Age", true)]
    [Theory]
    public void ArrayContainsInt(string code, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(code);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(Model));
    }

    [InlineData("['John', 'Bob'] contains $Survey.Name", false)]
    [InlineData("['Jacob DeGrom', 'Johnny Bench'] contains $Survey.Name", true)]
    [Theory]
    public void ArrayContainsString(string code, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(code);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(Model));
    }

    [InlineData("@GetAnswerArray() contains 20", false)]
    [InlineData("@GetAnswerArray() contains 2", true)]
    [Theory]
    public void ContainsFromMethod(string code, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(code);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(Model));
    }

    [InlineData("'abc' like 'def'", false)]
    [InlineData("'baseball' like 'base'", true)] //sql it's column name which is the longer text inside smaller text which we search for
    [Theory]
    public void StringLike(string code, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(code);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(Model));
    }

    #endregion

    #region Comparison Tests

    //age is 30 for these tests

    //equals
    [InlineData("$Survey.Age == 29", false)]
    [InlineData("$Survey.Age == 30", true)]

    //not equal
    [InlineData("$Survey.Age != 29", true)]
    [InlineData("$Survey.Age != 30", false)]
    [InlineData("$Survey.Age != 31", true)]

    //less then
    [InlineData("$Survey.Age < 31", true)]
    [InlineData("$Survey.Age < 30", false)]

    //less then or equal
    [InlineData("$Survey.Age <= 29", false)]
    [InlineData("$Survey.Age <= 30", true)]
    [InlineData("$Survey.Age <= 31", true)]

    //greater then
    [InlineData("$Survey.Age > 29", true)]
    [InlineData("$Survey.Age > 30", false)]
    [InlineData("$Survey.Age > 31", false)]

    //greater then or equal
    [InlineData("$Survey.Age >= 29", true)]
    [InlineData("$Survey.Age >= 30", true)]
    [InlineData("$Survey.Age >= 31", false)]

    //is null
    [InlineData("$Survey.NumberOfKidsNullable == null", true)]
    [InlineData("$Survey.NumberOfKidsNullable != null", false)]

    //nullable double
    [InlineData("$Survey.Price > 3.45d?", true)]
    [InlineData("$Survey.Price < 4.50d?", false)]

    //non-nullable double
    [InlineData("$Survey.PriceNonNullable > 3.45d", true)]
    [InlineData("$Survey.PriceNonNullable < 4.50d", false)]

    [Theory]
    public void ComparisonTest(string statementToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(statementToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(Model));
    }

    [InlineData("$Survey.Name == null", true)]
    [InlineData("$Survey.Name != null", false)]
    [Theory]
    public void NullStringComparisonTest(string statementToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(statementToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModel(null!, 0, false, null, null!)));
    }

    #endregion

    #region Combiner Tests [AndAlso OrElse]

    //nullable double
    [InlineData("[1d?,2d?,3d?, 35d?] contains $Survey.Price", true)]
    [InlineData("[1d?,2d?,3d?] contains $Survey.Price", false)]

    //nullable ints
    [InlineData("[1?,2?,3?, 5?] contains $Survey.NumberOfKidsNullable", true)]
    [InlineData("[1?,2?,3?] contains $Survey.NumberOfKidsNullable", false)]
    [Theory]
    public void NullableArrayContains(string statementToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(statementToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModel("Custom Name", 24, false, 5, new Dictionary<int, string>(), Price: 35)));
    }

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

    #region Misc

    [InlineData("$Survey.NumberOfKidsNullable > 1?", true)]
    [InlineData("$Survey.NumberOfKidsNullable < 5?", false)]
    [Theory]
    public void NullableIntWithValueTest(string statementToText, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(statementToText);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModel("John", 21, true, 10, new Dictionary<int, string>())));
    }

    [InlineData("$Survey.NullableBoolTest == true?", true)]
    [InlineData("$Survey.NullableBoolTest == false?", false)]
    [Theory]
    public void NullableBoolWithValueTest(string statementToText, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(statementToText);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModel("John", 21, true, 10, new Dictionary<int, string>(), true)));
    }

    [InlineData("$Survey.NullableBoolTest == true?", false)]
    [InlineData("$Survey.NullableBoolTest == false?", false)]
    [Theory]
    public void NullableBoolWithNullValueTest(string statementToText, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(statementToText);
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModel("John", 21, true, 10, new Dictionary<int, string>())));
    }

    #endregion

    #region Linq Statements

    [Fact]
    public void LinqWithExpression()
    {
        var dataSet = new List<SurveyModel>
        {
            new SurveyModel("John", 21, false, null, new Dictionary<int,string>{ {1, "NA" } }),
            new SurveyModel("Jacob", 30, true, null, new Dictionary<int,string>{ {1, "High" } }),
            new SurveyModel("Jason", 37, true, null, new Dictionary<int,string>{ {1, "Low" } })
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
            new SurveyModel("John", 21, false, null, new Dictionary<int,string>{ {1, "NA" } }),
            new SurveyModel("Jacob", 30, true, null, new Dictionary<int,string>{ {1, "High" } }),
            new SurveyModel("Jason", 37, true, null, new Dictionary<int,string>{ {1, "Low" } })
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
            new SurveyModel("John", 21, false, null, new Dictionary<int,string>{ {1, "NA" } }),
            new SurveyModel("Jacob", 30, true, null, new Dictionary<int,string>{ {1, "High" } }),
            new SurveyModel("Jason", 37, true, null, new Dictionary<int,string>{ {1, "Low" } })
        };

        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1(1) == 'High'");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel>(tokens, "Survey");

        var filteredDataSet = dataSet.AsQueryable().Where(expression).ToList();

        Assert.Single(filteredDataSet);
        Assert.Equal("Jacob", filteredDataSet.Single().Name);
    }

    #endregion

    #region None Or Multiple Parameters

    [Fact]
    public void NoParameterTest()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("1 == 2");
        var expression = RuleParserExpressionBuilder.BuildExpression(tokens);

        Assert.False(expression.Compile().Invoke());
    }

    [Fact]
    public void TwoParameterTest()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 && $Size.Age == 12");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel, SurveyModel>(tokens, "Survey", "Size");

        Assert.True(expression.Compile().Invoke(Model, new SurveyModel("Jacob", 12, false, null, new Dictionary<int, string>())));
    }

    [Fact]
    public void ThreeParameterTest()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.Age == 30 && $Size.Age == 12 && $Color.Age == 15");
        var expression = RuleParserExpressionBuilder.BuildExpression<SurveyModel, SurveyModel, SurveyModel>(tokens, "Survey", "Size", "Color");

        Assert.True(expression.Compile().Invoke(Model,
                                                new SurveyModel("Jacob", 12, false, null, new Dictionary<int, string>()),
                                                new SurveyModel("Teenager", 15, false, null, new Dictionary<int, string>())));
    }

    #endregion

}
