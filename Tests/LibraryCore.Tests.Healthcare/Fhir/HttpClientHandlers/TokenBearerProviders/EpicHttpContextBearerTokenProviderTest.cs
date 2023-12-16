using LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders.Implementations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LibraryCore.Tests.Healthcare.Fhir.HttpClientHandlers.TokenBearerProviders;

public class EpicHttpContextBearerTokenProviderTest
{
    [Fact]
    public async Task ThrowsIfHttpContextNull()
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var tokenProviderToTest = new EpicHttpContextBearerTokenProvider(httpContextAccessor.Object);

        var result = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            _ = await tokenProviderToTest.AccessTokenAsync(default);
        });

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public async Task CanGetTokenFromHttpContext()
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockIAuthenticationService = new Mock<IAuthenticationService>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        var tokenValueToExpect = Guid.NewGuid().ToString();

        mockServiceProvider.Setup(x => x.GetService(typeof(IAuthenticationService)))
          .Returns(mockIAuthenticationService.Object);

        mockHttpContextAccessor.Setup(x => x.HttpContext)
            .Returns(mockHttpContext.Object);

        mockHttpContext.Setup(x => x.RequestServices)
            .Returns(mockServiceProvider.Object);

        var authenticationResult = AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), "test"));

        authenticationResult.Properties?.StoreTokens(new[]
        {
             new AuthenticationToken{ Name = "access_token", Value = tokenValueToExpect }
        });

        mockIAuthenticationService.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), null))
            .Returns(Task.FromResult(authenticationResult));

        var result = await new EpicHttpContextBearerTokenProvider(mockHttpContextAccessor.Object).AccessTokenAsync(default);

        Assert.Equal(tokenValueToExpect, result);

        mockHttpContextAccessor.VerifyAll();
        mockHttpContext.VerifyAll();
        mockIAuthenticationService.VerifyAll();
        mockServiceProvider.VerifyAll();
    }
}
