using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services.Localization;

public class ResxImportService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IWebHostEnvironment _environment;

    public ResxImportService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IWebHostEnvironment environment)
    {
        _dbContextFactory = dbContextFactory;
        _environment = environment;
    }

    public async Task<ImportResult> ImportFromResxFilesAsync()
    {
        var result = new ImportResult();
        var resourcesPath = Path.Combine(_environment.ContentRootPath, "Resources");

        // Map culture to resx file names
        // Process culture-specific files first, then neutral/default
        var cultureFileMap = new List<(string Culture, string FileName)>
        {
            ("en-US", "SharedResource.en-US.resx"),
            ("vi-VN", "SharedResource.vi-VN.resx"),
            ("zh-CN", "SharedResource.zh-CN.resx"),
            ("en-US", "SharedResource.resx") // Default/neutral is en-US, process last to allow override
        };

        // Process each culture
        foreach (var (culture, fileName) in cultureFileMap)
        {
            var filePath = Path.Combine(resourcesPath, fileName);

            if (!File.Exists(filePath))
            {
                if (fileName != "SharedResource.resx") // Only warn for culture-specific files
                {
                    result.Warnings.Add($"File not found: {fileName}");
                }
                continue;
            }

            try
            {
                var resources = ParseResxFile(filePath);
                if (resources.Count > 0)
                {
                    var imported = await ImportResourcesAsync(resources, culture);
                    
                    result.ImportedCount += imported;
                    if (!result.ProcessedCultures.Contains(culture))
                    {
                        result.ProcessedCultures.Add(culture);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error processing {fileName}: {ex.Message}");
            }
        }

        // Clear cache after import
        DbStringLocalizerFactory.ClearCache();

        return result;
    }

    private Dictionary<string, string> ParseResxFile(string filePath)
    {
        var resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        var doc = XDocument.Load(filePath);
        var root = doc.Root;
        
        if (root == null)
        {
            return resources;
        }

        // Find all <data> elements
        var dataElements = root.Elements()
            .Where(e => e.Name.LocalName == "data")
            .ToList();

        foreach (var dataElement in dataElements)
        {
            var nameAttribute = dataElement.Attribute("name");
            if (nameAttribute == null || string.IsNullOrEmpty(nameAttribute.Value))
            {
                continue;
            }

            var key = nameAttribute.Value;
            var valueElement = dataElement.Element("value");
            
            if (valueElement != null)
            {
                var value = valueElement.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    resources[key] = value;
                }
            }
        }

        return resources;
    }

    private async Task<int> ImportResourcesAsync(Dictionary<string, string> resources, string culture)
    {
        using var context = _dbContextFactory.CreateDbContext();
        
        var importedCount = 0;
        var existingResources = await context.LocalizationResources
            .Where(r => r.Culture == culture)
            .ToDictionaryAsync(r => r.ResourceKey, r => r, StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in resources)
        {
            var resourceKey = kvp.Key;
            var value = kvp.Value;

            if (existingResources.TryGetValue(resourceKey, out var existing))
            {
                // Update existing resource
                if (existing.Value != value)
                {
                    existing.Value = value;
                    importedCount++;
                }
            }
            else
            {
                // Insert new resource
                var newResource = new LocalizationResource
                {
                    ResourceKey = resourceKey,
                    Culture = culture,
                    Value = value
                };
                context.LocalizationResources.Add(newResource);
                importedCount++;
            }
        }

        await context.SaveChangesAsync();
        return importedCount;
    }
}

public class ImportResult
{
    public int ImportedCount { get; set; }
    public List<string> ProcessedCultures { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
