﻿using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserBrokenCodeTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public RuleParserBrokenCodeTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [Fact]
    public void StringWithNoClosingBracket()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.RuleParserEngineToUse.ParseString("$Name == 'noclosingbracket"));

        Assert.Equal("Missing closing quote on String Value. Current Value = noclosingbracket", result.Message);
    }

    [Fact]
    public void NumberNotParseable()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.RuleParserEngineToUse.ParseString("$Id == 12345678912341231231232156789"));

        Assert.Equal("Number Factory Not Able To Parse Number. Value = 12345678912341231231232156789", result.Message);
    }

    [Fact]
    public void TrueStartsValidButEntireWordNotPresent()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.RuleParserEngineToUse.ParseString("$Id == trNOT"));

        Assert.Equal("Character Read N Is Not Expected. Expected Character = U or u", result.Message);
    }
}
