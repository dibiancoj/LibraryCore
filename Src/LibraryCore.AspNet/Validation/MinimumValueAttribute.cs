using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class MinimumValueAttribute(double minimumValueAccepted, bool allowNulls) : ValidationAttribute
{
    public MinimumValueAttribute(double minimumValueAccepted)
        : this(minimumValueAccepted, false)
    {
    }

    public override bool IsValid(object? value)
    {
        return value == null ?
            allowNulls :
            Convert.ToDouble(value) > minimumValueAccepted;
    }
}
