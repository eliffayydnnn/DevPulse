using System.Text.Json;
using DevPulse.Application.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DevPulse.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly string? _redisConnectionString;
    private IConnectionMultiplexer? _redisConnection;
    private bool _redisConnectionAttempted;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(
        IMemoryCache memoryCache,
        IConfiguration configuration,
        ILogger<RedisCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _redisConnectionString = configuration.GetConnectionString("Redis")
            ?? configuration["REDIS_CONNECTION"]
            ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION");
    }

    private IDatabase? GetDatabase()
    {
        if (string.IsNullOrWhiteSpace(_redisConnectionString))
        {
            return null;
        }

        if (_redisConnection != null && _redisConnection.IsConnected)
        {
            return _redisConnection.GetDatabase();
        }

        lock (_lock)
        {
            if (_redisConnectionAttempted && (_redisConnection == null || !_redisConnection.IsConnected))
            {
                return null;
            }

            try
            {
                _redisConnectionAttempted = true;
                var configOptions = ConfigurationOptions.Parse(_redisConnectionString);
                configOptions.ConnectTimeout = 2000;
                configOptions.SyncTimeout = 2000;
                configOptions.AbortOnConnectFail = false;

                _redisConnection = ConnectionMultiplexer.Connect(configOptions);
                if (_redisConnection.IsConnected)
                {
                    _logger.LogInformation("Successfully connected to Redis distributed cache server at {Endpoints}.", string.Join(", ", _redisConnection.GetEndPoints().Select(e => e.ToString())));
                    return _redisConnection.GetDatabase();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not establish connection to Redis ({Message}). Seamlessly falling back to IMemoryCache.", ex.Message);
            }
        }

        return null;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // 1. Try L1 Memory Cache first (sub-millisecond speed)
        if (_memoryCache.TryGetValue(key, out T? localValue))
        {
            return localValue;
        }

        // 2. Try Redis if available
        try
        {
            var db = GetDatabase();
            if (db != null)
            {
                var redisValue = await db.StringGetAsync(key);
                if (redisValue.HasValue)
                {
                    var deserialized = JsonSerializer.Deserialize<T>(redisValue.ToString()!, JsonOptions);
                    if (deserialized != null)
                    {
                        // Populate local cache for subsequent rapid reads
                        _memoryCache.Set(key, deserialized, TimeSpan.FromMinutes(2));
                        return deserialized;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Redis StringGetAsync failed for key {Key}. Using memory fallback.", key);
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var ttl = expiration ?? TimeSpan.FromMinutes(10);

        // Always populate memory cache
        _memoryCache.Set(key, value, ttl);

        // If Redis is active, populate Redis
        try
        {
            var db = GetDatabase();
            if (db != null)
            {
                var json = JsonSerializer.Serialize(value, JsonOptions);
                await db.StringSetAsync(key, json, ttl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Redis StringSetAsync failed for key {Key}. Stored in memory cache only.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);

        try
        {
            var db = GetDatabase();
            if (db != null)
            {
                await db.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Redis KeyDeleteAsync failed for key {Key}.", key);
        }
    }
}
