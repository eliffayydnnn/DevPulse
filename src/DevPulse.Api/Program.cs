using System.Text.Json.Serialization;
using DevPulse.Api.Middleware;
using DevPulse.Application;
using DevPulse.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register Clean Architecture layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Controllers with JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPulseCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevPulse API",
        Version = "v1",
        Description = "Enterprise Full-Stack GitHub Analytics SaaS Platform API. Provides real-time repository analytics, commit trends, PR/issue velocity, and multi-repo comparisons."
    });
});

var app = builder.Build();

// Centralized ProblemDetails exception handling
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "DevPulse API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("DevPulseCorsPolicy");

app.MapControllers();

app.Run();

// Make Program accessible for WebApplicationFactory integration tests
public partial class Program { }
