using System.Collections.Concurrent;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In-memory storage for submissions
var submissions = new ConcurrentDictionary<string, Submission>();

// Health check endpoint
app.MapGet("/healthz", () => new { status = "ok" })
    .WithName("HealthCheck");

// Service info endpoint
app.MapGet("/api/info", () => new { name = "review-policy", version = "0.1.0" })
    .WithName("ServiceInfo");

// Get all submissions
app.MapGet("/submissions", () => submissions.Values.ToArray())
    .WithName("GetSubmissions");

// Create submission
app.MapPost("/submissions", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    
    JsonDocument? jsonDoc = null;
    string appId = "";
    string version = "";
    string notes = "";
    
    if (!string.IsNullOrEmpty(body))
    {
        try
        {
            jsonDoc = JsonDocument.Parse(body);
            if (jsonDoc.RootElement.TryGetProperty("appId", out var appIdElement))
                appId = appIdElement.GetString() ?? "";
            if (jsonDoc.RootElement.TryGetProperty("version", out var versionElement))
                version = versionElement.GetString() ?? "";
            if (jsonDoc.RootElement.TryGetProperty("notes", out var notesElement))
                notes = notesElement.GetString() ?? "";
        }
        catch
        {
            return Results.BadRequest(new { error = "invalid_json" });
        }
    }
    
    if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(version))
    {
        return Results.BadRequest(new { error = "appId and version are required" });
    }
    
    var id = Guid.NewGuid().ToString();
    var submission = new Submission
    {
        Id = id,
        AppId = appId,
        Version = version,
        Notes = notes,
        Status = "received",
        CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    };
    
    submissions[id] = submission;
    
    context.Response.StatusCode = 201;
    context.Response.Headers.Location = $"/submissions/{id}";
    return Results.Json(submission, statusCode: 201);
})
.WithName("CreateSubmission");

// Get submission by id
app.MapGet("/submissions/{id}", (string id) =>
{
    if (submissions.TryGetValue(id, out var submission))
    {
        return Results.Json(submission);
    }
    
    return Results.Json(new { error = "not_found" }, statusCode: 404);
})
.WithName("GetSubmission");

app.Run();

public record Submission
{
    public string Id { get; init; } = "";
    public string AppId { get; init; } = "";
    public string Version { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
    public string CreatedAt { get; init; } = "";
}

public partial class Program { }
