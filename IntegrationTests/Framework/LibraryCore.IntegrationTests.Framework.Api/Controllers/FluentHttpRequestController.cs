using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FluentHttpRequestController : ControllerBase
{
    #region Models

    /// <summary>
    /// using a class because a record will throw xml serializer warnings.
    /// </summary>
    public class ResultModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
    }

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
    public ResultModel SimpleJsonPayload() => new() { Id = 9999, Text = "1111" };

    [HttpGet("SimpleXmlPayload")]
    public XmlRoot SimpleXmlPayload() => new() { Id = 5 };

    [HttpGet("SimpleJsonPayloadWithJsonParameters")]
    public ResultModel SimpleJsonPayloadWithJsonParameters([FromBody] ResultModel parameters) => new() { Id = parameters.Id + 1, Text = parameters.Text + "_Result" };

    [Consumes("application/x-www-form-urlencoded")]
    [HttpGet("FormsEncodedParameters")]
    public IEnumerable<ResultModel> FormsEncodedParameters([FromForm] IFormCollection parameters) => parameters.Select(t => new ResultModel() { Id = Convert.ToInt32(t.Key) + 10, Text = t.Value + "_Result" });

    [HttpPost("FileUploadStream")]
    public IEnumerable<ResultModel> FileUploadStream(IEnumerable<IFormFile> formFiles) => formFiles.Select(file => new ResultModel() { Id = Convert.ToInt32(file.Length), Text = file.FileName });
}