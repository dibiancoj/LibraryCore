using LibraryCore.AspNet.Attributes;
using LibraryCore.AspNet.Validation;
using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimpleController : Controller
    {
        [HttpGetOptionHead("AspNetHttpMethodTest")]
        public bool AspNetHttpMethodTest() => true;

        [HttpPost("ValidationTest")]
        public bool ValidationTest([FromBody] ValidationTest model) => true;
    }

    public class ValidationTest
    {
        [DateOfBirthRange]
        public DateTime? DataOfBirth { get; set; }

        [MaximumValue(100)]
        public int MaximumValue { get; set; }

        [MinimumValue(25)]
        public int MinimumValue { get; set; }

        [RequiredIf(nameof(RequiredIfValue), "IsRequired")]
        [PastDateValidation(false)]
        public DateTime? PastDateValue{ get; set; }

        public string? RequiredIfValue { get; set; }
    }
}
