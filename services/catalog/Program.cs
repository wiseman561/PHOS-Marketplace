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

// Health check endpoint
app.MapGet("/healthz", () => new { status = "ok" })
    .WithName("HealthCheck");

// Service info endpoint
app.MapGet("/api/info", () => new { name = "catalog", version = "0.1.0" })
    .WithName("ServiceInfo");

// Get all apps
app.MapGet("/apps", () => new object[0])
    .WithName("GetApps");

// Create app
app.MapPost("/apps", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    
    JsonDocument? jsonDoc = null;
    string name = "untitled";
    string description = "";
    
    if (!string.IsNullOrEmpty(body))
    {
        try
        {
            jsonDoc = JsonDocument.Parse(body);
            if (jsonDoc.RootElement.TryGetProperty("name", out var nameElement))
                name = nameElement.GetString() ?? "untitled";
            if (jsonDoc.RootElement.TryGetProperty("description", out var descElement))
                description = descElement.GetString() ?? "";
        }
        catch
        {
            // If JSON parsing fails, use defaults
        }
    }
    
    var id = Guid.NewGuid().ToString();
    var response = new { id, name, description };
    
    context.Response.StatusCode = 201;
    context.Response.Headers.Location = $"/apps/{id}";
    return Results.Json(response, statusCode: 201);
})
.WithName("CreateApp");

// Get app by id
app.MapGet("/apps/{id}", (string id) =>
{
    if (id == "demo")
    {
        return Results.Json(new { id = "demo", name = "Demo App", description = "placeholder" });
    }
    
    return Results.Json(new { error = "not_found" }, statusCode: 404);
})
.WithName("GetApp");

app.Run();

public partial class Program { }