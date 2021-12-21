using LibraryCore.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace LibraryCore.IntegrationTests
{
    public class AspNetValidationIntegrationTest : IClassFixture<WebApplicationFactoryFixture>
    {
        public AspNetValidationIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture)
        {
            WebApplicationFactoryFixture = webApplicationFactoryFixture;
        }

        private WebApplicationFactoryFixture WebApplicationFactoryFixture { get; }

        [InlineData(true, -2)]
        [InlineData(false, -500)]
        [Theory]
        public async Task DateOfBirthTest(bool expectedToBeSuccessful, int howManyYearsAgoWereTheyBorn)
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("Simple/ValidationTest", new ValidationTest { DataOfBirth = DateTime.Now.AddYears(howManyYearsAgoWereTheyBorn) });

            Assert.Equal(expectedToBeSuccessful, response.IsSuccessStatusCode);
            Assert.Equal(expectedToBeSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        }

        [InlineData(true, 50)]
        [InlineData(false, 105)]
        [Theory]
        public async Task MaxValueTest(bool expectedToBeSuccessful, int maxValueToUse)
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("Simple/ValidationTest", new ValidationTest { MaximumValue = maxValueToUse });

            Assert.Equal(expectedToBeSuccessful, response.IsSuccessStatusCode);
            Assert.Equal(expectedToBeSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        }

        [InlineData(true, 50)]
        [InlineData(false, 1)]
        [Theory]
        public async Task MinimumValueTest(bool expectedToBeSuccessful, int maxValueToUse)
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("Simple/ValidationTest", new ValidationTest { MinimumValue = maxValueToUse });

            Assert.Equal(expectedToBeSuccessful, response.IsSuccessStatusCode);
            Assert.Equal(expectedToBeSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        }

        [InlineData(true, true)]
        [InlineData(false, false)]
        [Theory]
        public async Task PastDateValueTest(bool expectedToBeSuccessful, bool addPastDate)
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("Simple/ValidationTest", new ValidationTest { PastDateValue = addPastDate ? DateTime.Now.AddDays(-2) : DateTime.Now.AddDays(2) });

            Assert.Equal(expectedToBeSuccessful, response.IsSuccessStatusCode);
            Assert.Equal(expectedToBeSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    public class ValidationTest
    {
        public DateTime? DataOfBirth { get; set; } = DateTime.Now.AddYears(-2);

        public int MaximumValue { get; set; } = 50;

        public int MinimumValue { get; set; } = 50;

        public DateTime? PastDateValue { get; set; }
    }
}
