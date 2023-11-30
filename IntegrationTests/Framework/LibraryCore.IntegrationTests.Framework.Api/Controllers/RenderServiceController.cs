using LibraryCore.AspNet.Render;
using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RenderServiceController(IRenderService renderService) : Controller
{
    private IRenderService RenderService { get; } = renderService;

    [HttpGet("CantFindView")]
    public async Task<IActionResult> CantFindView() => Content(await RenderService.RenderToStringAsync("~/Views/RenderService/CantFindView.cshtml"));

    [HttpGet("WithModel")]
    public async Task<IActionResult> WithModel() => Content(await RenderService.RenderToStringAsync("~/Views/RenderService/WithModel.cshtml", new KeyValuePair<string, string>("key1", "value1")));

    [HttpGet("NoModel")]
    public async Task<IActionResult> NoModel() => Content(await RenderService.RenderToStringAsync("~/Views/RenderService/NoModel.cshtml"));
}
