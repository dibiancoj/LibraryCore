using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LibraryCore.AspNet.Validation;

public class USZipCodeValidationAttribute : ValidationAttribute
{
    public USZipCodeValidationAttribute(bool required)
    {
        Required = required;
    }

    private bool Required { get; }

    public override bool IsValid(object? value)
    {
        if (value == null || value is not string tryCastToString || string.IsNullOrWhiteSpace(tryCastToString))
        {
            return !Required;
        }

        return tryCastToString.Length == 5 && tryCastToString.All(x => char.IsNumber(x));
    }
}
