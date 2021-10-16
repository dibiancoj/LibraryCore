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
            return actionDescriptor is not ControllerActionDescriptor castedAction ?
                    false :
                    castedAction.MethodInfo.IsDefined(typeof(TAttribute), true);
        }
    }
}
