using System.Collections.Concurrent;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

// Seed catalog (static for MVP)
var apps = new[]
{
    new StoreApp("app.genomekit", "GenomeKit", "Genomics insights"),
    new StoreApp("app.nutritionkit", "NutritionKit", "Nutrition planning"),
    new StoreApp("app.microbiomekit", "MicrobiomeKit", "Gut microbiome guidance")
};

// In-memory installs
var installs = new ConcurrentDictionary<string, Install>(); // id -> Install

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/info", () => Results.Ok(new { name = "appstore", version = "0.1.0" }));

app.MapGet("/api/store/apps", () => Results.Ok(apps));

app.MapPost("/api/store/apps/{appId}/install", (string appId, InstallRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.UserId))
        return Results.BadRequest(new Error("user_required"));

    if (!apps.Any(a => a.Id == appId))
        return Results.NotFound(new Error("not_found"));

    var id = Guid.NewGuid().ToString();
    var install = new Install(id, req.UserId, appId, DateTimeOffset.UtcNow);
    installs[id] = install;
    return Results.Created($"/api/store/installs/{id}", install);
});

app.MapGet("/api/store/installs", (string? userId) =>
{
    var query = installs.Values.AsEnumerable();
    if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(i => i.UserId == userId);
    return Results.Ok(query.ToArray());
});

app.MapGet("/api/store/installs/{id}", (string id) =>
    installs.TryGetValue(id, out var install)
        ? Results.Ok(install)
        : Results.NotFound(new Error("not_found"))
);

app.Run();

public record StoreApp(string Id, string Name, string Description);
public record Install(string Id, string UserId, string AppId, DateTimeOffset InstalledAt);
public record InstallRequest([property: JsonPropertyName("userId")] string UserId);
public record Error(string error);

public partial class Program { }
