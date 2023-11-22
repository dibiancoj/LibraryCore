using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class PastDateValidationAttribute(bool required) : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return !required;
        }

        return DateTime.TryParse(value.ToString(), out DateTime userGivenDate) && userGivenDate.Date <= DateTime.Today;
    }

}
