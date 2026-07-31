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
                    mainLayoutBackgroundColor = preference.MainLayoutBackgroundColor,
                    fontHeaderH6 = preference.FontHeaderH6,
                    fontTieuDe = preference.FontTieuDe,
                    fontNoiDung = preference.FontNoiDung,
                    fontNoiDungLuoi = preference.FontNoiDungLuoi,
                    fontNavMenu = preference.FontNavMenu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving theme preference", error = ex.Message });
            }
        }

        [HttpPost("update-fonts")]
        public async Task<IActionResult> UpdateContentFontPreference([FromBody] UpdateContentFontRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.FontHeaderH6) ||
                    string.IsNullOrWhiteSpace(request.FontTieuDe) ||
                    string.IsNullOrWhiteSpace(request.FontNoiDung) ||
                    string.IsNullOrWhiteSpace(request.FontNoiDungLuoi))
                {
                    return BadRequest(new { message = "Invalid request data" });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var success = await _themeService.UpdateUserContentFontPreferenceAsync(
                    userId.Value,
                    request.FontHeaderH6.Trim(),
                    request.FontTieuDe.Trim(),
                    request.FontNoiDung.Trim(),
                    request.FontNoiDungLuoi.Trim());

                if (success)
                    return Ok(new { message = "Font preference updated successfully" });
                else
                    return StatusCode(500, new { message = "Failed to update font preference" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating font preference", error = ex.Message });
            }
        }

        [HttpPost("update-navmenu-font")]
        public async Task<IActionResult> UpdateNavMenuFontPreference([FromBody] UpdateNavMenuFontRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.FontNavMenu))
                    return BadRequest(new { message = "Invalid request data" });

                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "User not found" });

                var success = await _themeService.UpdateUserNavMenuFontPreferenceAsync(
                    userId.Value,
                    request.FontNavMenu.Trim());

                if (success)
                    return Ok(new { message = "Nav menu font preference updated successfully" });
                else
                    return StatusCode(500, new { message = "Failed to update nav menu font preference" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating nav menu font preference", error = ex.Message });
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

    public class UpdateContentFontRequest
    {
        public string FontHeaderH6 { get; set; } = string.Empty;
        public string FontTieuDe { get; set; } = string.Empty;
        public string FontNoiDung { get; set; } = string.Empty;
        public string FontNoiDungLuoi { get; set; } = string.Empty;
    }

    public class UpdateNavMenuFontRequest
    {
        public string FontNavMenu { get; set; } = string.Empty;
    }
}
