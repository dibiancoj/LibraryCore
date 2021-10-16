using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation
{
    /// <summary>
    /// Field is required if a another property which is ienumerable contains a specific value
    /// </summary>
    public class RequiredIfContainsAttribute : ValidationAttribute
    {
        public RequiredIfContainsAttribute(string propertyName, object requiredIfValue)
        {
            RequiredIfPropertyName = propertyName;
            RequiredIfValue = requiredIfValue;
        }

        private string RequiredIfPropertyName { get; }
        private object RequiredIfValue { get; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            //try to short circuit without reflection. Do we have a value then we can ignore everything
            if (!string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return ValidationResult.Success;
            }

            var triggerPropertyInfo = validationContext.ObjectInstance.GetType().GetProperty(RequiredIfPropertyName) ?? throw new MissingFieldException($"Property Name = {RequiredIfPropertyName} not found in object");

            var triggerPropertyValue = triggerPropertyInfo.GetValue(validationContext.ObjectInstance, null);

            if (triggerPropertyValue is string || triggerPropertyValue is not IEnumerable castToIEnumerable)
            {
                throw new Exception("Trigger Property Value Is Not A List. Property Name = " + RequiredIfPropertyName);
            }

            var enumerator = castToIEnumerable.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Equals(RequiredIfValue))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
