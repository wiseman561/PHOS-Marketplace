using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace AppsHost.Tests;

public class AppsHostHealthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public AppsHostHealthTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Health_Works() {
        var r = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }
}
