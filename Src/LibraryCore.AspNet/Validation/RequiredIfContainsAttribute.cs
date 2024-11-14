using LibraryCore.Shared;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.AspNet.Validation;

/// <summary>
/// Field is required if a another property which is ienumerable contains a specific value
/// </summary>

public class RequiredIfContainsAttribute(string propertyName, object requiredIfValue) : ValidationAttribute
{
    [RequiresUnreferencedCode($"{nameof(IsValid)} It Not Trimmer Safe")]
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        //try to short circuit without reflection. Do we have a value then we can ignore everything
        if (!string.IsNullOrWhiteSpace(value?.ToString()))
        {
            return ValidationResult.Success;
        }

        var triggerPropertyInfo = validationContext.ObjectInstance.GetType().GetProperty(propertyName) ?? throw new MissingFieldException($"Property Name = {propertyName} not found in object");

        var triggerPropertyValue = triggerPropertyInfo.GetValue(validationContext.ObjectInstance, null);

        if (triggerPropertyValue is string || triggerPropertyValue is not IEnumerable castToIEnumerable)
        {
            throw new Exception("Trigger Property Value Is Not A List. Property Name = " + propertyName);
        }

        var enumerator = castToIEnumerable.GetEnumerator();

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Equals(requiredIfValue))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}
