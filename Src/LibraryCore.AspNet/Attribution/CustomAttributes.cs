using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;

namespace LibraryCore.AspNet.Attribution
{
    public static class CustomAttributes
    {
        public static bool CustomAttributeIsDefined<TAttribute>(ActionDescriptor actionDescriptor)
            where TAttribute : Attribute
        {
            var castedAction = actionDescriptor as ControllerActionDescriptor;

            return castedAction == null ?
                    false :
                    castedAction.MethodInfo.IsDefined(typeof(TAttribute), true);
        }
    }
}
