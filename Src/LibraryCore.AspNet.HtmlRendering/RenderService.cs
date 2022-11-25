using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace LibraryCore.AspNet.Render;

public class RenderService : IRenderService
{

    #region Constructor

    public RenderService(IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor accessor)
    {
        RazorViewEngine = razorViewEngine;
        TempDataProvider = tempDataProvider;
        Accessor = accessor;
    }

    #endregion

    #region Properties

    private IRazorViewEngine RazorViewEngine { get; }
    private ITempDataProvider TempDataProvider { get; }
    private IHttpContextAccessor Accessor { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Render a view or partial view to a string when you don't have a model
    /// </summary>
    /// <param name="fullpathToViewOrPartial">view or partial name to render. Use the full path of the view ie: ~/Areas/Patient/Views/Home/Index.cshtml</param>
    /// <returns>Task which will return a string</returns>
    public async Task<string> RenderToStringAsync(string fullpathToViewOrPartial)
    {
        return await RenderToStringAsync(fullpathToViewOrPartial, null, null, null, null);
    }

    /// <summary>
    /// Render a view or partial view to a string
    /// </summary>
    /// <param name="fullpathToViewOrPartial">view or partial name to render. Use the full path of the view ie: ~/Areas/Patient/Views/Home/Index.cshtml</param>
    /// <param name="model">model to render with. Null if no model</param>
    /// <returns>Task which will return a string</returns>
    public async Task<string> RenderToStringAsync(string fullpathToViewOrPartial, object model)
    {
        return await RenderToStringAsync(fullpathToViewOrPartial, model, null, null, null);
    }

    /// <summary>
    /// Render a view or partial view to a string
    /// </summary>
    /// <param name="fullpathToViewOrPartial">view or partial name to render. Use the full path of the view ie: ~/Areas/Patient/Views/Home/Index.cshtml</param>
    /// <param name="model">model to render with. Null if no model</param>
    /// <param name="modelStateDictionary">model state dictionary</param>
    /// <returns>Task which will return a string</returns>
    public async Task<string> RenderToStringAsync(string fullpathToViewOrPartial, object? model, ModelStateDictionary? modelStateDictionary, RouteData? routeData, ActionDescriptor? actionDescriptor)
    {
        var actionContext = new ActionContext(Accessor.HttpContext ?? throw new NullReferenceException("HttpContext Not Found In Accessor"), routeData ?? new RouteData(), actionDescriptor ?? new ActionDescriptor());

        using var outputWriter = new StringWriter();

        //since we have area's we need to reference the full path. This was the only way i can get it to work
        //await RenderService.RenderToStringAsync("~/Areas/Patient/Views/Home/Index.cshtml");
        //await RenderService.RenderToStringAsync("~/Areas/Patient/Views/Shared/_TopNavMenu.cshtml", true);

        var viewResult = RazorViewEngine.GetView(executingFilePath: null, viewPath: fullpathToViewOrPartial, isMainPage: false);

        if (!viewResult.Success)
        {
            throw new ArgumentNullException($"{fullpathToViewOrPartial} does not match any available view. No view found.");
        }

        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelStateDictionary ?? new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, TempDataProvider),
            outputWriter,
            new HtmlHelperOptions()
        )
        {
            RouteData = Accessor.HttpContext.GetRouteData()
        };

        await viewResult.View.RenderAsync(viewContext);

        return outputWriter.ToString();
    }

    #endregion

}

