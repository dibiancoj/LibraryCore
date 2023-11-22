using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class USZipCodeValidationAttribute(bool required) : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null || value is not string tryCastToString || string.IsNullOrWhiteSpace(tryCastToString))
        {
            return !required;
        }

        return tryCastToString.Length == 5 && tryCastToString.All(x => char.IsNumber(x));
    }
}
