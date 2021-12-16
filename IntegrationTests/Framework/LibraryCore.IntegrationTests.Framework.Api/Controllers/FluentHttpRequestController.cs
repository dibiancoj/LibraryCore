using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FluentHttpRequestController : ControllerBase
    {

        #region Models
        public record ResultModel(int Id, string Text);

        public class XmlRoot
        {
            public int Id { get; set; }
        }

        #endregion

        [HttpGet("HeaderTest")]
        public string HeaderTest() => $"{Request.Headers["MyHeader"].First()} Result";

        [HttpGet("QueryStringTest")]
        public string QueryStringTest() => $"{Request.Query["Q1"]}:{Request.Query["Q2"]}";

        [HttpGet("SimpleJsonPayload")]
        public ResultModel SimpleJsonPayload() => new(9999, "1111");

        [HttpGet("SimpleXmlPayload")]
        public XmlRoot SimpleXmlPayload() => new() { Id = 5 };

        [HttpGet("SimpleJsonPayloadWithJsonParameters")]
        public ResultModel SimpleJsonPayloadWithJsonParameters([FromBody] ResultModel parameters) => new(parameters.Id + 1, parameters.Text + "_Result");

        [Consumes("application/x-www-form-urlencoded")]
        [HttpGet("FormsEncodedParameters")]
        public IEnumerable<ResultModel> FormsEncodedParameters([FromForm] IFormCollection parameters) => parameters.Select(t => new ResultModel(Convert.ToInt32(t.Key) + 10, t.Value + "_Result"));

        [HttpPost("FileUploadStream")]
        public IEnumerable<ResultModel> FileUploadStream() => Request.Form.Files.Select(file => new ResultModel(Convert.ToInt32(file.Length), file.FileName));
    }
}