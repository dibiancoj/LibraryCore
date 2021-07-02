using LibraryCore.Core.DataTypes;
using System;
using Xunit;

namespace LibraryCore.Tests.Core.DataTypes
{
    public class AccountingPeriodTest
    {

        [Fact]
        public void BuildPeriodFromDate()
        {
            var temp = new AccountingPeriod(new DateTime(2020, 5, 1));

            Assert.Equal(2020, temp.Year);
            Assert.Equal(5, temp.Month);
            Assert.Equal(202005, temp.ToFullAccountingPeriod());
            Assert.Equal(new DateTime(2020, 5, 1), temp.ToDateTime());
        }

        [Fact]
        public void BuildPeriodFromNumbers()
        {
            var temp = new AccountingPeriod(5, 2020);

            Assert.Equal(2020, temp.Year);
            Assert.Equal(5, temp.Month);
            Assert.Equal(202005, temp.ToFullAccountingPeriod());
            Assert.Equal(new DateTime(2020, 5, 1), temp.ToDateTime());
        }

        [Fact]
        public void BuildPeriodFromNumbersWithInvalidMonth()
        {
            Assert.Throws<Exception>(() => new AccountingPeriod(13, 2020));
        }

        [Fact]
        public void BuildPeriodFromNumbersWithInvalidYear()
        {
            Assert.Throws<Exception>(() => new AccountingPeriod(05, 1));
        }

        [Fact]
        public void BuildPeriodFromFullAccountingPeriod()
        {
            var temp = new AccountingPeriod(202005);

            Assert.Equal(2020, temp.Year);
            Assert.Equal(5, temp.Month);
            Assert.Equal(202005, temp.ToFullAccountingPeriod());
            Assert.Equal(new DateTime(2020, 5, 1), temp.ToDateTime());
        }

        [Fact]
        public void BuildPeriodFromFullAccountingPeriodWithInvalidMonth()
        {
            Assert.Throws<Exception>(() => new AccountingPeriod(202015));
        }

        [Fact]
        public void BuildPeriodFromFullAccountingPeriodWithInvalidYear()
        {
            Assert.Throws<Exception>(() => new AccountingPeriod(05012));
        }

        [InlineData(202001, 1, 202002)] //1 month
        [InlineData(202001, 3, 202004)] //3 month
        [InlineData(202005, 12, 202105)] //full year
        [Theory]
        public void IncrementPeriods(int accountPeriod, int periodsToIncrement, int expectedResult)
        {
            Assert.Equal(expectedResult, (new AccountingPeriod(accountPeriod) + periodsToIncrement).ToFullAccountingPeriod());
        }

        [InlineData(202002, 1, 202001)] //-1 month
        [InlineData(202003, 3, 201912)] //-3 month
        [InlineData(202105, 12, 202005)] //minus full year
        [Theory]
        public void SubtractPeriods(int accountPeriod, int periodsToIncrement, int expectedResult)
        {
            Assert.Equal(expectedResult, (new AccountingPeriod(accountPeriod) - periodsToIncrement).ToFullAccountingPeriod());
        }

    }
}
