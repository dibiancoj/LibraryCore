using LibraryCore.AspNet.ExtensionMethods.HttpContextExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LibraryCore.Tests.AspNet.ExtensionMethods.HttpContextExtensionMethods;

public class HttpRequestExtensionsTest
{

    #region Framework

    public class MyController : Controller
    {
    }

    #endregion

    #region Unit Tests

    [Fact(DisplayName = "Is not ajax request")]
    public void IsAjaxWhenRequestIsNotAjaxCall()
    {
        var httpContext = new DefaultHttpContext();

        Assert.False(httpContext.Request.IsAjaxRequest());
    }

    [Fact(DisplayName = "Is an ajax request")]
    public void IsAjaxWhenRequestIsAjaxCall()
    {
        var controller = new MyController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        Assert.True(controller.Request.IsAjaxRequest());
    }

    #endregion

}
