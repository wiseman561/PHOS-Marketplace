using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Gateway.Tests;

public class GatewayHealthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public GatewayHealthTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Works()
    {
        var resp = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("ok", body);
    }

    [Fact]
    public async Task Info_Works()
    {
        var resp = await _client.GetAsync("/api/info");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("gateway", body);
    }
}
