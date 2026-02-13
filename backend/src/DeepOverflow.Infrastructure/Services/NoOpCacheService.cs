using System.Collections.Concurrent;
using DeepOverflow.Application.Common.Interfaces;

namespace DeepOverflow.Infrastructure.Services;

public sealed class NoOpCacheService : ICacheService
{
    private sealed record CacheItem(object Value, DateTimeOffset? ExpiresAt);

    private readonly ConcurrentDictionary<string, CacheItem> _cache = new(StringComparer.Ordinal);

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(key, out var item))
            return Task.FromResult<T?>(default);

        if (item.ExpiresAt.HasValue && item.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            _cache.TryRemove(key, out _);
            return Task.FromResult<T?>(default);
        }

        if (item.Value is T typed) return Task.FromResult<T?>(typed);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        DateTimeOffset? expiresAt = expiration.HasValue
            ? DateTimeOffset.UtcNow.Add(expiration.Value)
            : (DateTimeOffset?)null;
        _cache[key] = new CacheItem(value!, expiresAt);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        foreach (var key in _cache.Keys)
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
            {
                _cache.TryRemove(key, out _);
            }
        }
        return Task.CompletedTask;
    }
}

