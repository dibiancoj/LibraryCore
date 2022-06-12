using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class PastDateValidationAttribute : ValidationAttribute
{
    public PastDateValidationAttribute(bool required)
    {
        Required = required;
    }

    private bool Required { get; }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return !Required;
        }

        return DateTime.TryParse(value.ToString(), out DateTime userGivenDate) && userGivenDate.Date <= DateTime.Today;
    }

}
