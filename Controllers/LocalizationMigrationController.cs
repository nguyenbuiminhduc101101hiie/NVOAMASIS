using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVOAMASIS.Services.Localization;

namespace NVOAMASIS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Protect this endpoint - only authorized users can import localization data
public class LocalizationMigrationController : ControllerBase
{
    private readonly ResxImportService _importService;
    private readonly ILogger<LocalizationMigrationController> _logger;

    public LocalizationMigrationController(
        ResxImportService importService,
        ILogger<LocalizationMigrationController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportFromResx()
    {
        try
        {
            _logger.LogInformation("Starting localization import from RESX files");
            
            var result = await _importService.ImportFromResxFilesAsync();

            _logger.LogInformation(
                "Import completed. Imported: {Count}, Cultures: {Cultures}, Warnings: {Warnings}, Errors: {Errors}",
                result.ImportedCount,
                string.Join(", ", result.ProcessedCultures),
                result.Warnings.Count,
                result.Errors.Count);

            return Ok(new
            {
                success = true,
                importedCount = result.ImportedCount,
                processedCultures = result.ProcessedCultures,
                warnings = result.Warnings,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during localization import");
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message
            });
        }
    }
}
