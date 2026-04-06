using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Services;
using System.Security.Claims;

namespace NVOAMASIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ThemeController : ControllerBase
    {
        private readonly ThemeService _themeService;
        private readonly AppDbContext _context;

        public ThemeController(ThemeService themeService, AppDbContext context)
        {
            _themeService = themeService;
            _context = context;
        }

        private Guid? GetCurrentUserId()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return null;

            var user = _context.UserList
                .AsNoTracking()
                .FirstOrDefault(u => u.Name == userName);

            return user?.UsrId;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetThemePreference()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var preference = await _themeService.GetUserThemePreferenceAsync(userId.Value);
                return Ok(new
                {
                    navMenuBackgroundColor = preference.NavMenuBackgroundColor,
                    navMenuTextColor = preference.NavMenuTextColor,
                    mainLayoutBackgroundColor = preference.MainLayoutBackgroundColor
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving theme preference", error = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateThemePreference([FromBody] UpdateThemeRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.NavMenuBackgroundColor) ||
                    string.IsNullOrWhiteSpace(request.NavMenuTextColor) ||
                    string.IsNullOrWhiteSpace(request.MainLayoutBackgroundColor))
                {
                    return BadRequest(new { message = "Invalid request data" });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var success = await _themeService.UpdateUserThemePreferenceAsync(
                    userId.Value,
                    request.NavMenuBackgroundColor,
                    request.NavMenuTextColor,
                    request.MainLayoutBackgroundColor);

                if (success)
                    return Ok(new { message = "Theme preference updated successfully" });
                else
                    return StatusCode(500, new { message = "Failed to update theme preference" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating theme preference", error = ex.Message });
            }
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetThemePreference()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var success = await _themeService.ResetToDefaultAsync(userId.Value);
                if (success)
                    return Ok(new { message = "Theme preference reset to default" });
                else
                    return StatusCode(500, new { message = "Failed to reset theme preference" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error resetting theme preference", error = ex.Message });
            }
        }
    }

    public class UpdateThemeRequest
    {
        public string NavMenuBackgroundColor { get; set; } = string.Empty;
        public string NavMenuTextColor { get; set; } = string.Empty;
        public string MainLayoutBackgroundColor { get; set; } = string.Empty;
    }
}
