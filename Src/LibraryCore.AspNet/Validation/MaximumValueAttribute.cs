using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class MaximumValueAttribute(double maximumValueAccepted, bool allowNulls) : ValidationAttribute
{
    public MaximumValueAttribute(double maximumValueAccepted)
        : this(maximumValueAccepted, false)
    {
    }

    public override bool IsValid(object? value)
    {
        return value == null ?
                allowNulls :
                Convert.ToDouble(value) <= maximumValueAccepted;
    }
}
