using System;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Core.DataTypes;

public class AccountingPeriod
{

    #region Constructors

    public AccountingPeriod(int month, int year)
    {
        //this is the base constructor
        Month = IsValidMonthAndRaiseError(month);
        Year = IsValidYearAndRaiseError(year);
        FullAccountingPeriod = Convert.ToInt32($"{Year}{Month:D2}");
    }

    public AccountingPeriod(DateOnly accountingPeriodToParse)
        : this(IsValidMonthAndRaiseError(accountingPeriodToParse.Month), IsValidYearAndRaiseError(accountingPeriodToParse.Year))
    {
    }

    #endregion

    #region Properties

    public int Month { get; }
    public int Year { get; }
    public int FullAccountingPeriod { get; }

    #endregion

    #region Operators

    public static AccountingPeriod operator +(AccountingPeriod a, int b)
    {
        return new AccountingPeriod(a.ToDate().AddMonths(b));
    }

    public static AccountingPeriod operator -(AccountingPeriod a, int b)
    {
        return new AccountingPeriod(a.ToDate().AddMonths(-b));
    }

    #endregion

    #region Public Methods

    public DateOnly ToDate() => new(Year, Month, 1);

    public static bool TryParseAccountingPeriod(int fullAccountingPeriod, [NotNullWhen(true)] out AccountingPeriod? tryParseAccountingPeriod)
    {
        if (!IsValidFullAccountingPeriod(fullAccountingPeriod))
        {
            tryParseAccountingPeriod = default;
            return false;
        }

        var temp = fullAccountingPeriod.ToString().AsSpan();

        int tempMonth = Convert.ToInt32(new string(temp.Slice(4, 2)));
        int tempYear = Convert.ToInt32(new string(temp[..4]));

        if (!IsValidMonth(tempMonth) || !IsValidYear(tempYear))
        {
            tryParseAccountingPeriod = default;
            return false;
        }

        tryParseAccountingPeriod = new AccountingPeriod(tempMonth, tempYear);

        return true;
    }

    public static AccountingPeriod ParseAccountingPeriod(int fullAccountingPeriod)
    {
        if (!TryParseAccountingPeriod(fullAccountingPeriod, out var tryToParseValue))
        {
            throw new Exception($"Invalid Accounting Period. Value = {fullAccountingPeriod}");
        }

        return tryToParseValue;
    }

    #endregion

    #region Private Validation Methods

    private static bool IsValidFullAccountingPeriod(int fullAccountingPeriod) => fullAccountingPeriod >= 190001 & fullAccountingPeriod < 300001;

    private static bool IsValidMonth(int monthToValidate) => monthToValidate > 0 && monthToValidate < 13;

    private static bool IsValidYear(int yearToValidate) => yearToValidate >= 1900 & yearToValidate < 3000;

    private static int IsValidMonthAndRaiseError(int monthToValidate)
    {
        return IsValidMonth(monthToValidate) ?
            monthToValidate :
            throw new Exception($"Invalid Month Format. Expected MM - Got {monthToValidate}");
    }

    private static int IsValidYearAndRaiseError(int yearToValidate)
    {
        return IsValidYear(yearToValidate) ?
            yearToValidate :
            throw new Exception($"Invalid Year Format. Expected YYYY - Got {yearToValidate}");
    }

    #endregion

}
