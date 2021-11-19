using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class MinimumValueAttribute : ValidationAttribute
{
    public MinimumValueAttribute(double minimumValueAccepted)
        : this(minimumValueAccepted, false)
    {
    }

    public MinimumValueAttribute(double minimumValueAccepted, bool allowNulls)
    {
        MinimumValueAccepted = minimumValueAccepted;
        AllowNullValues = allowNulls;
    }

    private double MinimumValueAccepted { get; }
    private bool AllowNullValues { get; }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return AllowNullValues;
        }

        return Convert.ToDouble(value) > MinimumValueAccepted;
    }
}
