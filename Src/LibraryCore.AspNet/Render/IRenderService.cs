using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace LibraryCore.AspNet.Render;

public interface IRenderService
{
    /// <summary>
    /// Render a view or partial view to a string when you don't have a model
    /// </summary>
    /// <param name="fullpathToViewOrPartial">view or partial name to render. Use the full path of the view ie: ~/Areas/Patient/Views/Home/Index.cshtml</param>
    /// <returns>Task which will return a string</returns>
    Task<string> RenderToStringAsync(string fullpathToViewOrPartial);

    /// <summary>
    /// Render a view or partial view to a string
    /// </summary>
    /// <param name="fullpathToViewOrPartial">view or partial name to render. Use the full path of the view ie: ~/Areas/Patient/Views/Home/Index.cshtml</param>
    /// <param name="model">model to render with. Null if no model</param>
    /// <returns>Task which will return a string</returns>
    Task<string> RenderToStringAsync(string fullpathToViewOrPartial, object model);

    /// <summary>
    /// Render a view or partial view to a string
    /// </summary>
    /// <param name="fullpathToViewOrPartial">view or partial name to render. Use the full path of the view ie: ~/Areas/Patient/Views/Home/Index.cshtml</param>
    /// <param name="model">model to render with. Null if no model</param>
    /// <param name="modelStateDictionary">Model state</param>
    /// <param name="routeData">Route data if applicable</param>
    /// <param name="actionDescriptor">action descriptor if applicable</param>
    /// <returns>Task which will return a string</returns>
    Task<string> RenderToStringAsync(string fullpathToViewOrPartial, object? model, ModelStateDictionary? modelStateDictionary, RouteData? routeData, ActionDescriptor? actionDescriptor);
}