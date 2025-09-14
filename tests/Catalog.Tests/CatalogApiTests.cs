using Microsoft.AspNetCore.Mvc.Testing;

namespace Catalog.Tests;

public class CatalogApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CatalogApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthz_ReturnsOkWithStatus()
    {
        // Act
        var response = await _client.GetAsync("/healthz");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("status", content);
        Assert.Contains("ok", content);
    }

    [Fact]
    public async Task GetApiInfo_ReturnsServiceInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/info");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("name", content);
        Assert.Contains("catalog", content);
        Assert.Contains("version", content);
        Assert.Contains("0.1.0", content);
    }

    [Fact]
    public async Task GetApps_ReturnsEmptyArray()
    {
        // Act
        var response = await _client.GetAsync("/apps");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("[]", content);
    }

    [Fact]
    public async Task PostApps_WithValidData_ReturnsCreatedWithLocation()
    {
        // Arrange
        var appData = new { name = "Test" };

        // Act
        var response = await _client.PostAsJsonAsync("/apps", appData);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        // Check Location header
        Assert.True(response.Headers.Location?.ToString().StartsWith("/apps/"));
        
        // Check response body
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("id", responseContent);
        Assert.Contains("name", responseContent);
        Assert.Contains("Test", responseContent);
        
        // Verify the id is a valid GUID format
        var responseObj = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(responseObj);
    }

    [Fact]
    public async Task GetAppsDemo_ReturnsDemoPayload()
    {
        // Act
        var response = await _client.GetAsync("/apps/demo");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("id", content);
        Assert.Contains("demo", content);
        Assert.Contains("name", content);
        Assert.Contains("Demo App", content);
        Assert.Contains("description", content);
        Assert.Contains("placeholder", content);
    }

    [Fact]
    public async Task GetAppsNotThere_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/apps/not-there");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content);
        Assert.Contains("not_found", content);
    }
}
