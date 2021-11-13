using LibraryCore.Core.DataTypes;
using System;
using Xunit;

namespace LibraryCore.Tests.Core.DataTypes;

public class AccountingPeriodTest
{

    [Fact]
    public void BuildPeriodFromDate()
    {
        var temp = new AccountingPeriod(new DateOnly(2020, 5, 1));

        Assert.Equal(2020, temp.Year);
        Assert.Equal(5, temp.Month);
        Assert.Equal(202005, temp.FullAccountingPeriod);
        Assert.Equal(new DateOnly(2020, 5, 1), temp.ToDate());
    }

    [Fact]
    public void BuildPeriodFromNumbers()
    {
        var temp = new AccountingPeriod(5, 2020);

        Assert.Equal(2020, temp.Year);
        Assert.Equal(5, temp.Month);
        Assert.Equal(202005, temp.FullAccountingPeriod);
        Assert.Equal(new DateOnly(2020, 5, 1), temp.ToDate());
    }

    [Fact]
    public void BuildPeriodFromNumbersWithInvalidMonth()
    {
        Assert.Throws<Exception>(() => new AccountingPeriod(13, 2020));
    }

    [Fact]
    public void ParseAccountingPeriodThrows()
    {
        Assert.Throws<Exception>(() => AccountingPeriod.ParseAccountingPeriod(202114));
    }

    [Fact]
    public void ParseAccountingPeriodReturnsCorrectly()
    {
        var result = AccountingPeriod.ParseAccountingPeriod(202112);

        Assert.Equal(2021, result.Year);
        Assert.Equal(12, result.Month);
        Assert.Equal(202112, result.FullAccountingPeriod);
    }

    [Fact]
    public void BuildPeriodFromNumbersWithInvalidYear()
    {
        Assert.Throws<Exception>(() => new AccountingPeriod(05, 1));
    }

    [Fact]
    public void TryParseFullAccountingPeriod()
    {
        Assert.True(AccountingPeriod.TryParseAccountingPeriod(202005, out var tryToParseValue));

        Assert.Equal(2020, tryToParseValue.Year);
        Assert.Equal(5, tryToParseValue.Month);
        Assert.Equal(202005, tryToParseValue.FullAccountingPeriod);
        Assert.Equal(new DateOnly(2020, 5, 1), tryToParseValue.ToDate());
    }

    [Fact]
    public void TryParseFullAccountingPeriodWithInvalidMonth()
    {
        Assert.False(AccountingPeriod.TryParseAccountingPeriod(202015, out var _));
    }

    [Fact]
    public void BuildPeriodFromFullAccountingPeriodWithInvalidYear()
    {
        Assert.False(AccountingPeriod.TryParseAccountingPeriod(05012, out var _));
    }

    [InlineData(202001, 1, 202002)] //1 month
    [InlineData(202001, 3, 202004)] //3 month
    [InlineData(202005, 12, 202105)] //full year
    [Theory]
    public void IncrementPeriods(int accountPeriod, int periodsToIncrement, int expectedResult)
    {
        Assert.Equal(expectedResult, (AccountingPeriod.ParseAccountingPeriod(accountPeriod) + periodsToIncrement).FullAccountingPeriod);
    }

    [InlineData(202002, 1, 202001)] //-1 month
    [InlineData(202003, 3, 201912)] //-3 month
    [InlineData(202105, 12, 202005)] //minus full year
    [Theory]
    public void SubtractPeriods(int accountPeriod, int periodsToIncrement, int expectedResult)
    {
        Assert.Equal(expectedResult, (AccountingPeriod.ParseAccountingPeriod(accountPeriod) - periodsToIncrement).FullAccountingPeriod);
    }

}
