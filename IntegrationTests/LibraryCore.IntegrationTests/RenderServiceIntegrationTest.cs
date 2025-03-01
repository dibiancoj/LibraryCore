﻿using LibraryCore.IntegrationTests.Fixtures;

namespace LibraryCore.IntegrationTests;

public class RenderServiceIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture) : IClassFixture<WebApplicationFactoryFixture>
{
    private WebApplicationFactoryFixture WebApplicationFactoryFixture { get; } = webApplicationFactoryFixture;

    [Fact]
    public async Task CantFindView()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.GetAsync("RenderService/CantFindView", TestContext.Current.CancellationToken);

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task RenderServiceWithNoModel()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.GetAsync("RenderService/NoModel", TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("<div>Test</div>", result, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task RenderServiceWithModel()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.GetAsync("RenderService/WithModel", TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("\r\n<div aria-key=key1 aria-value=value1>\r\n</div>", result, ignoreLineEndingDifferences: true);
    }
}
