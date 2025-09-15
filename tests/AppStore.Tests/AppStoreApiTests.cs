using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AppStore.Tests;

public class AppStoreApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public AppStoreApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Health_Works()
    {
        var r = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var t = await r.Content.ReadAsStringAsync();
        Assert.Contains("ok", t);
    }

    [Fact]
    public async Task ListApps_ReturnsSeededApps()
    {
        var r = await _client.GetAsync("/api/store/apps");
        r.EnsureSuccessStatusCode();
        var items = await r.Content.ReadFromJsonAsync<StoreApp[]>();
        Assert.NotNull(items);
        Assert.True(items!.Length >= 3);
    }

    [Fact]
    public async Task Install_Then_ListAndGet()
    {
        var create = await _client.PostAsJsonAsync("/api/store/apps/app.genomekit/install", new { userId = "u1" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<Install>();
        Assert.NotNull(created);

        var list = await _client.GetFromJsonAsync<Install[]>("/api/store/installs?userId=u1");
        Assert.NotNull(list);
        Assert.Contains(list!, i => i!.Id == created!.Id);

        var get = await _client.GetAsync($"/api/store/installs/{created!.Id}");
        get.EnsureSuccessStatusCode();
    }
}

public record StoreApp(string Id, string Name, string Description);
public record Install(string Id, string UserId, string AppId, DateTimeOffset InstalledAt);
