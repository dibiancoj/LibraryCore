using LibraryCore.AspNet.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimpleController : Controller
    {
        [HttpGetOptionHead("AspNetHttpMethodTest")]
        public bool AspNetHttpMethodTest() => true;
    }
}
