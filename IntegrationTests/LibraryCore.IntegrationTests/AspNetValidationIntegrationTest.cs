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

        [InlineData(true, "", false)] //required field is not set so it should be valid
        [InlineData(true, "IsRequired", true)] //required field says past date should be valid and it is. So it should be valid
        [InlineData(false, "IsRequired", false)] //required field says past date should be valid. Past date is null so it should be a 400
        [Theory]
        public async Task RequiredIfTest(bool expectedToBeSuccessful, string requiredIfFieldValue, bool setPastDate)
        {
            var pastDateValue = setPastDate ? DateTime.Now.AddDays(-2) : new DateTime?();

            var response = await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("Simple/ValidationTest", new ValidationTest { RequiredIfValue = requiredIfFieldValue, PastDateValue = pastDateValue });

            Assert.Equal(expectedToBeSuccessful, response.IsSuccessStatusCode);
            Assert.Equal(expectedToBeSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        }

        [InlineData(true, "", false)] //required field is not set so it should be valid
        [InlineData(true, "IsRequired", true)] //required field says past date should be valid and it is. So it should be valid
        [InlineData(true, "SomeRandomText", false)] //text doesn't match whats in the list so the required field is not valid
        [InlineData(false, "IsRequired", false)] //required field says past date should be valid. Past date is null so it should be a 400
        [Theory]
        public async Task RequiredIfContainsTest(bool expectedToBeSuccessful, string requiredIfFieldValue, bool setTargetValue)
        {
            string? requiredIfContainsTarget = setTargetValue ? "some text": null;

            var response = await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("Simple/ValidationTest", new ValidationTest { RequiredIfContainsValue = new[] { requiredIfFieldValue }, RequiredIfContainsTarget = requiredIfContainsTarget });

            Assert.Equal(expectedToBeSuccessful, response.IsSuccessStatusCode);
            Assert.Equal(expectedToBeSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        }

        [InlineData(true, "")]
        [InlineData(false, "123")]
        [InlineData(false, "abc")]
        [InlineData(false, "1055134343")]
        [InlineData(true, "10551")]
        [Theory]
        public async Task ZipCodeTest(bool expectedToBeSuccessful, string zipCodeValue)
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("Simple/ValidationTest", new ValidationTest { ZipCodeValue = zipCodeValue });

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
        public string? RequiredIfValue { get; set; }

        public string? RequiredIfContainsTarget { get; set; }
        public IEnumerable<string> RequiredIfContainsValue { get; set; } = Array.Empty<string>();
        public string? ZipCodeValue { get; set; }
    }
}
