using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FluentHttpRequestController : ControllerBase
    {
        public record ResultModel(int Id, string Text);

        [HttpGet("SimpleJsonPayload")]
        public ResultModel Get() => new (9999, "1111");
    }
}