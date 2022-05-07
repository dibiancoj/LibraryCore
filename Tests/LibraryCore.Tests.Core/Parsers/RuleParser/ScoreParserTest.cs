﻿using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class ScoreParserTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public ScoreParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    public record ScoreParserModel(int Age);

    public enum RiskOfHeartAttackScore
    {
        Low,
        Medium,
        High
    }

    [Fact]
    public void ScoreModeIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = RuleParserFixture.ResolveRuleParserEngine()
                                          .ParseScoreNew<string>(
                                                  new("a", "$User.Age$ < 18"),
                                                  new("b", "$User.Age$ >= 18 && $User.Age$ < 20"),
                                                  new("c", "$User.Age$ >= 20"))
                                          .BuildScoreExpression<ScoreParserModel>(ScoreToken.ScoringMode.AccumulatedScore, "User")
                                          .Compile();
        });
    }

    [InlineData(-1, 3)]
    [InlineData(25, 18)]
    [InlineData(25, 19)]
    [InlineData(50, 20)]
    [Theory]
    public void IntBasedScore_ShortCircuitOnFirstTrueEval(int expectedScore, int Age)
    {
        var compiledExpression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseScoreNew<int>(
                                                    new(-1, "$User.Age$ < 18"),
                                                    new(25, "$User.Age$ >= 18 && $User.Age$ < 20"),
                                                    new(50, "$User.Age$ >= 20"))
                                            .BuildScoreExpression<ScoreParserModel>(ScoreToken.ScoringMode.ShortCircuitOnFirstTrueEval, "User")
                                            .Compile();

        var result = compiledExpression(new ScoreParserModel(Age));

        Assert.Equal(expectedScore, result);
    }

    [InlineData(0, 3)]
    [InlineData(1, 4)]
    [InlineData(26, 11)]
    [InlineData(76, 30)]
    [InlineData(51, 101)]
    [Theory]
    public void IntBasedScore_AccumulatedScore(int expectedScore, int Age)
    {
        var compiledExpression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseScoreNew<int>(
                                                    new(1, "$User.Age$ > 3"),
                                                    new(25, "$User.Age$ > 10 && $User.Age$ < 100"),
                                                    new(50, "$User.Age$ > 20"))
                                            .BuildScoreExpression<ScoreParserModel>(ScoreToken.ScoringMode.AccumulatedScore, "User")
                                            .Compile();

        var result = compiledExpression(new ScoreParserModel(Age));

        Assert.Equal(expectedScore, result);
    }

    [InlineData("A", 10)]
    [InlineData("B", 20)]
    [InlineData("C", 30)]
    [InlineData("D", 40)]
    [Theory]
    public void StringBasedScore_ShortCircuitOnFirstTrueEval(string expectedScore, int Age)
    {
        var compiledExpression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseScoreNew<string>(
                                                    new("A", "$User.Age$ == 10"),
                                                    new("B", "$User.Age$ == 20"),
                                                    new("C", "$User.Age$ == 30"),
                                                    new("D", "$User.Age$ == 40"))
                                            .BuildScoreExpression<ScoreParserModel>(ScoreToken.ScoringMode.ShortCircuitOnFirstTrueEval, "User")
                                            .Compile();

        var result = compiledExpression(new ScoreParserModel(Age));

        Assert.Equal(expectedScore, result);
    }

    [InlineData(RiskOfHeartAttackScore.Low, 15)]
    [InlineData(RiskOfHeartAttackScore.Medium, 30)]
    [InlineData(RiskOfHeartAttackScore.High, 60)]
    [Theory]
    public void EnumBasedScore_ShortCircuitOnFirstTrueEval(RiskOfHeartAttackScore expectedScore, int Age)
    {
        var compiledExpression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseScoreNew<RiskOfHeartAttackScore>(
                                                    new(RiskOfHeartAttackScore.Low, "$User.Age$ < 20"),
                                                    new(RiskOfHeartAttackScore.Medium, "$User.Age$ >= 21 && $User.Age$ < 40"),
                                                    new(RiskOfHeartAttackScore.High, "$User.Age$ >= 40"))
                                            .BuildScoreExpression<ScoreParserModel>(ScoreToken.ScoringMode.ShortCircuitOnFirstTrueEval, "User")
                                            .Compile();

        var result = compiledExpression(new ScoreParserModel(Age));

        Assert.Equal(expectedScore, result);
    }

    [InlineData(-1, 3)]
    [InlineData(25, 18)]
    [InlineData(25, 19)]
    [InlineData(50, 20)]
    [Theory]
    public void WithMethodCall(int expectedScore, int Age)
    {
        var compiledExpression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseScoreNew<int>(
                                                    new(-1, "@ScoreParser.GetAge($User$) < 18"),
                                                    new(25, "@ScoreParser.GetAge($User$) >= 18 && @ScoreParser.GetAge($User$) < 20"),
                                                    new(50, "@ScoreParser.GetAge($User$) >= 20"))
                                            .BuildScoreExpression<ScoreParserModel>(ScoreToken.ScoringMode.ShortCircuitOnFirstTrueEval, "User")
                                            .Compile();

        var result = compiledExpression(new ScoreParserModel(Age));

        Assert.Equal(expectedScore, result);
    }
}
