using System;

namespace LibraryCore.Core.DataTypes
{
    public class AccountingPeriod
    {

        #region Constructors

        public AccountingPeriod(DateTime accountingPeriodToParse)
        {
            Month = ValidateMonthOrThrow(accountingPeriodToParse.Month);
            Year = ValidateYearOrThrow(accountingPeriodToParse.Year);
        }

        public AccountingPeriod(int month, int year)
        {
            Month = ValidateMonthOrThrow(month);
            Year = ValidateYearOrThrow(year);
        }

        public AccountingPeriod(int fullAccountingPeriod)
        {
            var temp = TryParseFullAccountingPeriod(fullAccountingPeriod);

            Year = ValidateYearOrThrow(Convert.ToInt32(new string(temp.Slice(0, 4))));
            Month = ValidateMonthOrThrow(Convert.ToInt32(new string(temp.Slice(4, 2))));
        }

        #endregion

        #region Properties

        public int Month { get; }
        public int Year { get; }

        #endregion

        #region Operators

        public static AccountingPeriod operator +(AccountingPeriod a, int b)
        {
            return new AccountingPeriod(a.ToDateTime().AddMonths(b));
        }

        public static AccountingPeriod operator -(AccountingPeriod a, int b)
        {
            return new AccountingPeriod(a.ToDateTime().AddMonths(-b));
        }

        #endregion

        #region Methods

        public DateTime ToDateTime() => new(Year, Month, 1);

        public int ToFullAccountingPeriod() => Convert.ToInt32($"{Year}{Month.ToString("D2")}");

        #endregion

        #region Private Validation Methods

        private static ReadOnlySpan<char> TryParseFullAccountingPeriod(int fullAccountingPeriod)
        {
            var temp = fullAccountingPeriod.ToString().AsSpan();

            return temp.Length != 6 ?
                 throw new Exception($"Invalid Format. Expected YYYYMM - Got {fullAccountingPeriod}") :
                 temp;
        }

        private static int ValidateYearOrThrow(int yearToValidate)
        {
            return yearToValidate.ToString().Length != 4 ?
                throw new Exception($"Invalid Year Format. Expected YYYY - Got {yearToValidate}") :
                yearToValidate;
        }

        private static int ValidateMonthOrThrow(int monthToValidate)
        {
            return monthToValidate < 1 || monthToValidate > 12 ?
                throw new Exception($"Invalid Month Format. Expected MM - Got {monthToValidate}") :
                monthToValidate;
        }

        #endregion

    }
}
