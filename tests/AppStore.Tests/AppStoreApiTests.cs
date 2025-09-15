using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AppStore.Tests;

public class AppStoreApiTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly HttpClient _client;
  public AppStoreApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

  [Fact] public async Task Health_Works() {
    var r = await _client.GetAsync("/healthz");
    Assert.Equal(HttpStatusCode.OK, r.StatusCode);
  }

  [Fact] public async Task ListApps_SeedsPresent() {
    var apps = await _client.GetFromJsonAsync<StoreApp[]>("/api/store/apps");
    Assert.NotNull(apps); Assert.True(apps!.Length >= 3);
  }

  [Fact] public async Task Install_Then_ListAnd_Get() {
    var resp = await _client.PostAsJsonAsync("/api/store/apps/app.genomekit/install", new { userId="u1" });
    Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    var inst = await resp.Content.ReadFromJsonAsync<Install>();
    Assert.NotNull(inst);
    var list = await _client.GetFromJsonAsync<Install[]>("/api/store/installs?userId=u1");
    Assert.Contains(list!, i => i.Id == inst!.Id);
    var get = await _client.GetAsync($"/api/store/installs/{inst!.Id}");
    get.EnsureSuccessStatusCode();
  }
}

public record StoreApp(string Id,string Name,string Description);
public record Install(string Id,string UserId,string AppId,DateTimeOffset InstalledAt);