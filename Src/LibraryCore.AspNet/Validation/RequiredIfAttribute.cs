using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LibraryCore.AspNet.Validation
{
    public class RequiredIfAttribute : ValidationAttribute
    {
        //###could do it this way instead of grabbing the istring localizer like we do below###

        //   services.AddSingleton<Microsoft.AspNetCore.Mvc.DataAnnotations.IValidationAttributeAdapterProvider, Library.AspNet.Validation.CustomValidationAttributeAdapterProvider>();

        //public class RequiredIfAttributeAdapter : AttributeAdapterBase<RequiredIfAttribute>
        //{
        //    public RequiredIfAttributeAdapter(RequiredIfAttribute attribute, IStringLocalizer stringLocalizer) : base(attribute, stringLocalizer) { }

        //    public override void AddValidation(ClientModelValidationContext context) { }

        //    public override string GetErrorMessage(ModelValidationContextBase validationContext)
        //    {
        //        return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
        //    }
        //}

        //public class CustomValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
        //{
        //    private readonly IValidationAttributeAdapterProvider _baseProvider = new ValidationAttributeAdapterProvider();

        //    public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
        //    {
        //        if (attribute is RequiredIfAttribute)
        //            return new RequiredIfAttributeAdapter(attribute as RequiredIfAttribute, stringLocalizer);
        //        else
        //            return _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
        //    }
        //}

        public RequiredIfAttribute(string propertyName, params object[] requiredIfValue)
        {
            RequiredIfPropertyName = propertyName;
            RequiredIfValue = requiredIfValue;
        }

        private string RequiredIfPropertyName { get; }
        private object[] RequiredIfValue { get; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            //try to short circuit without reflection. Do we have a value then we can ignore everything
            if (!string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return ValidationResult.Success;
            }

            var triggerPropertyInfo = validationContext.ObjectInstance.GetType().GetProperty(RequiredIfPropertyName) ?? throw new Exception("Property Name = " + RequiredIfPropertyName + " not found in object");

            var triggerPropertyValue = triggerPropertyInfo.GetValue(validationContext.ObjectInstance, null);

            //trigger property is null
            if (triggerPropertyValue == null || !RequiredIfValue.Contains(triggerPropertyValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }

    }
}
