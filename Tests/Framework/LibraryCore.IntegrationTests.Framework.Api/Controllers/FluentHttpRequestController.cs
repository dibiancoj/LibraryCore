using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FluentHttpRequestController : ControllerBase
    {
        public record ResultModel(int Id, string Text);

        [HttpGet("HeaderTest")]
        public string HeaderTest() => $"{Request.Headers["MyHeader"].First()} Result";

        [HttpGet("SimpleJsonPayload")]
        public ResultModel SimpleJsonPayload() => new (9999, "1111");

        [HttpGet("SimpleJsonPayloadWithJsonParameters")]
        public ResultModel SimpleJsonPayloadWithJsonParameters([FromBody] ResultModel parameters) => new(parameters.Id + 1, parameters.Text + "_Result");

        [Consumes("application/x-www-form-urlencoded")]
        [HttpGet("FormsEncodedParameters")]
        public IEnumerable<ResultModel> FormsEncodedParameters([FromForm] IFormCollection parameters) => parameters.Select(t => new ResultModel(Convert.ToInt32(t.Key) + 10, t.Value + "_Result"));
    }
}