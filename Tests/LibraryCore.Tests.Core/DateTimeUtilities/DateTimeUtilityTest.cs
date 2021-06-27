using LibraryCore.Core.DateTimeUtilities;
using System;
using Xunit;

namespace LibraryCore.Tests.Core.DateTimeUtilities
{
    public class DateTimeUtilityTest
    {
        [Fact]
        public void CalculateAgeTest1()
        {
            Assert.Equal(0, DateTimeUtility.Age(DateTime.Today.AddDays(-5)));
        }

        [Fact]
        public void CalculateAgeTest2()
        {
            Assert.Equal(1, DateTimeUtility.Age(DateTime.Today.AddYears(-1).AddDays(-1)));
        }

        [Fact]
        public void CalculateAgeTest3()
        {
            Assert.Equal(2, DateTimeUtility.Age(DateTime.Today.AddYears(-2)));
        }

        [Fact]
        public void CalculateAgeTest4()
        {
            Assert.Equal(2, DateTimeUtility.Age(DateTime.Today.AddYears(-2).AddDays(-3)));
        }
    }
}
