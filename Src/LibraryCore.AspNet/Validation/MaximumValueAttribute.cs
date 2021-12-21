using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class MaximumValueAttribute : ValidationAttribute
{
    public MaximumValueAttribute(double maximumValueAccepted)
        : this(maximumValueAccepted, false)
    {
    }

    public MaximumValueAttribute(double maximumValueAccepted, bool allowNulls)
    {
        MaximumValueAccepted = maximumValueAccepted;
        AllowNullValues = allowNulls;
    }

    private double MaximumValueAccepted { get; }
    private bool AllowNullValues { get; }

    public override bool IsValid(object? value)
    {
        return value == null ?
                AllowNullValues :
                Convert.ToDouble(value) <= MaximumValueAccepted;
    }
}
