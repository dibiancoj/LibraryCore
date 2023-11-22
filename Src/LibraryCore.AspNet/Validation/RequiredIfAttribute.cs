using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class RequiredIfAttribute(string propertyName, params object[] requiredIfValue) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        //try to short circuit without reflection. Do we have a value then we can ignore everything
        if (!string.IsNullOrWhiteSpace(value?.ToString()))
        {
            return ValidationResult.Success;
        }

        var triggerPropertyInfo = validationContext.ObjectInstance.GetType().GetProperty(propertyName) ?? throw new MissingFieldException($"Property Name = {propertyName} not found in object");

        var triggerPropertyValue = triggerPropertyInfo.GetValue(validationContext.ObjectInstance, null);

        //trigger property is null
        if (triggerPropertyValue == null || !requiredIfValue.Contains(triggerPropertyValue))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage);
    }

}
