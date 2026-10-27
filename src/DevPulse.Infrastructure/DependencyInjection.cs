using DevPulse.Application.Contracts;
using DevPulse.Infrastructure.GitHub;
using DevPulse.Infrastructure.Persistence;
using DevPulse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevPulse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database connection (SQL Server / SQLEXPRESS / LocalDB)
        var connectionString = configuration.GetConnectionString("DevPulseDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=DevPulseDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

        services.AddDbContext<DevPulseDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        });

        // Memory Caching
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // GitHub API Typed HttpClient
        services.AddHttpClient<IGitHubService, GitHubClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.Timeout = TimeSpan.FromSeconds(25);
        });

        // Analytics & Business Services
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ISavedRepositoryService, SavedRepositoryService>();

        return services;
    }
}
