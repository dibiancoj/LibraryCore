using LibraryCore.AspNet.Render;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LibraryCore.Tests.AspNet.Render;

public class RenderServiceTest
{
    private const string viewWriteHtmlValue = "Test 123";

    private class TestView : IView
    {
        public string Path => string.Empty;

        public Task RenderAsync(ViewContext context)
        {
            return context.Writer.WriteLineAsync(viewWriteHtmlValue);
        }
    }

    [Fact(DisplayName = "Render View Without A Model")]
    public async Task RenderViewWithOutAModelToString()
    {
        var mockIRazorViewEngine = new Mock<IRazorViewEngine>();
        var mockITempDataProvider = new Mock<ITempDataProvider>();
        var mockIServiceProvider = new Mock<IServiceProvider>();
        var mockIHttpContextAccessor = new Mock<IHttpContextAccessor>();

        mockIRazorViewEngine.Setup(x => x.GetView(null, "Home//Test", false))
            .Returns(ViewEngineResult.Found("TestView", new TestView()));

        mockIHttpContextAccessor.Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext());

        IRenderService renderService = new RenderService(mockIRazorViewEngine.Object, mockITempDataProvider.Object, mockIHttpContextAccessor.Object);

        var result = await renderService.RenderToStringAsync("Home//Test");

        Assert.Equal(viewWriteHtmlValue + Environment.NewLine, result);
    }

    [Fact(DisplayName = "Can't Find View")]
    public async Task RenderViewToStringCantFindView()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        {
            var mockIRazorViewEngine = new Mock<IRazorViewEngine>();
            var mockITempDataProvider = new Mock<ITempDataProvider>();
            var mockIServiceProvider = new Mock<IServiceProvider>();
            var mockIHttpContextAccessor = new Mock<IHttpContextAccessor>();

            mockIRazorViewEngine.Setup(x => x.GetView(null, "Home//Test", false))
                .Returns(ViewEngineResult.NotFound("TestView", new[] { "Views", "Shared" }));

            mockIHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            IRenderService renderService = new RenderService(mockIRazorViewEngine.Object, mockITempDataProvider.Object, mockIHttpContextAccessor.Object);

            return renderService.RenderToStringAsync("Home//Test", "Test 123");
        });
    }

    [Fact(DisplayName = "Render View To String")]
    public async Task RenderViewToString()
    {
        var mockIRazorViewEngine = new Mock<IRazorViewEngine>();
        var mockITempDataProvider = new Mock<ITempDataProvider>();
        var mockIServiceProvider = new Mock<IServiceProvider>();
        var mockIHttpContextAccessor = new Mock<IHttpContextAccessor>();

        mockIRazorViewEngine.Setup(x => x.GetView(null, "Home//Test", false))
            .Returns(ViewEngineResult.Found("TestView", new TestView()));

        mockIHttpContextAccessor.Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext());

        IRenderService renderService = new RenderService(mockIRazorViewEngine.Object, mockITempDataProvider.Object, mockIHttpContextAccessor.Object);

        var result = await renderService.RenderToStringAsync("Home//Test", "Test 123");

        Assert.Equal(viewWriteHtmlValue + Environment.NewLine, result);
    }
}
