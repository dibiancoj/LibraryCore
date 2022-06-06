using LibraryCore.AspNet.Attribution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace LibraryCore.Tests.AspNet.Attribution;

public class CustomAttributeTest
{

    #region Framework

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    private class MyTestAttribute : Attribute
    {
    }

    private class TestController : Controller
    {
        public IActionResult Index() { throw new NotImplementedException(); }

        [MyTest]
        public IActionResult IndexWithAttribute() { throw new NotImplementedException(); }
    }

    #endregion

    #region Unit Tests

    [Fact]
    public void CustomAttributeIsDefined()
    {
        Assert.True(CustomAttributes.CustomAttributeIsDefined<MyTestAttribute>(new ControllerActionDescriptor { MethodInfo = typeof(TestController).GetMethod(nameof(TestController.IndexWithAttribute)) ?? throw new Exception("Can't Find Method") }));
    }

    [Fact]
    public void CustomAttributeIsNotDefined()
    {
        Assert.False(CustomAttributes.CustomAttributeIsDefined<MyTestAttribute>(new ControllerActionDescriptor { MethodInfo = typeof(TestController).GetMethod(nameof(TestController.Index)) ?? throw new Exception("Can't Find Method") }));
    }

    [Fact]
    public void CustomAttributeNullControllerActionDescriptor()
    {
        Assert.False(CustomAttributes.CustomAttributeIsDefined<MyTestAttribute>(null));
    }

    #endregion

}
