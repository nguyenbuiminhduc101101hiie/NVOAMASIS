using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services.Localization;

public class DbStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    
    // Cache resources by culture to avoid repeated database queries
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _resourceCache = new();
    private static readonly object _cacheLock = new object();

    public DbStringLocalizerFactory(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _dbContextFactory = dbContextFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var culture = GetCurrentCulture();
        var resourceName = resourceSource.Name; // e.g., "SharedResource"
        
        return CreateLocalizer(resourceName, culture);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var culture = GetCurrentCulture();
        var resourceName = string.IsNullOrEmpty(location) ? baseName : $"{baseName}.{location}";
        
        return CreateLocalizer(resourceName, culture);
    }

    private IStringLocalizer CreateLocalizer(string resourceName, string culture)
    {
        var cacheKey = $"{resourceName}:{culture}";
        
        if (!_resourceCache.TryGetValue(cacheKey, out var resources))
        {
            lock (_cacheLock)
            {
                // Double-check after acquiring lock
                if (!_resourceCache.TryGetValue(cacheKey, out resources))
                {
                    resources = LoadResourcesFromDatabase(culture);
                    _resourceCache.TryAdd(cacheKey, resources);
                }
            }
        }

        return new DbStringLocalizer(resources, culture, returnOnlyKeyIfNotFound: true);
    }

    private Dictionary<string, string> LoadResourcesFromDatabase(string culture)
    {
        using var context = _dbContextFactory.CreateDbContext();
        
        var resources = context.LocalizationResources
            .Where(r => r.Culture == culture)
            .ToDictionary(
                r => r.ResourceKey,
                r => r.Value,
                StringComparer.OrdinalIgnoreCase
            );

        return resources;
    }

    private string GetCurrentCulture()
    {
        // Try to get culture from HttpContext first (for Blazor Server)
        if (_httpContextAccessor?.HttpContext != null)
        {
            var requestCulture = _httpContextAccessor.HttpContext.Features
                .Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>();
            
            if (requestCulture != null)
            {
                return requestCulture.RequestCulture.UICulture.Name;
            }
        }

        // Fallback to CurrentUICulture
        return CultureInfo.CurrentUICulture.Name;
    }

    /// <summary>
    /// Clear the resource cache. Useful when resources are updated in the database.
    /// </summary>
    public static void ClearCache()
    {
        lock (_cacheLock)
        {
            _resourceCache.Clear();
        }
    }

    /// <summary>
    /// Clear cache for a specific culture.
    /// </summary>
    public static void ClearCache(string culture)
    {
        lock (_cacheLock)
        {
            var keysToRemove = _resourceCache.Keys
                .Where(k => k.EndsWith($":{culture}", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                _resourceCache.TryRemove(key, out _);
            }
        }
    }
}
