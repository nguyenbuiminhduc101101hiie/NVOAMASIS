using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services
{
    public class ThemeService
    {
        private readonly AppDbContext _context;

        public ThemeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserThemePreference> GetUserThemePreferenceAsync(Guid userId)
        {
            var preference = await _context.UserThemePreferences
                .FirstOrDefaultAsync(p => p.UsrId == userId);

            if (preference == null)
            {
                preference = CreateDefaultPreference(userId);
                _context.UserThemePreferences.Add(preference);
                await _context.SaveChangesAsync();
            }

            return preference;
        }

        public async Task<bool> UpdateUserThemePreferenceAsync(Guid userId, string navMenuColor, string navMenuTextColor, string mainLayoutColor)
        {
            try
            {
                var preference = await _context.UserThemePreferences
                    .FirstOrDefaultAsync(p => p.UsrId == userId);

                if (preference == null)
                {
                    preference = CreateDefaultPreference(userId);
                    preference.NavMenuBackgroundColor = navMenuColor;
                    preference.NavMenuTextColor = navMenuTextColor;
                    preference.MainLayoutBackgroundColor = mainLayoutColor;
                    _context.UserThemePreferences.Add(preference);
                }
                else
                {
                    preference.NavMenuBackgroundColor = navMenuColor;
                    preference.NavMenuTextColor = navMenuTextColor;
                    preference.MainLayoutBackgroundColor = mainLayoutColor;
                    preference.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating theme preference: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateUserContentFontPreferenceAsync(
            Guid userId,
            string fontHeaderH6,
            string fontTieuDe,
            string fontNoiDung,
            string fontNoiDungLuoi)
        {
            try
            {
                var preference = await GetOrCreatePreferenceAsync(userId);
                preference.FontHeaderH6 = fontHeaderH6;
                preference.FontTieuDe = fontTieuDe;
                preference.FontNoiDung = fontNoiDung;
                preference.FontNoiDungLuoi = fontNoiDungLuoi;
                preference.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating content font preference: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateUserNavMenuFontPreferenceAsync(Guid userId, string fontNavMenu)
        {
            try
            {
                var preference = await GetOrCreatePreferenceAsync(userId);
                preference.FontNavMenu = fontNavMenu;
                preference.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating nav menu font preference: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        private async Task<UserThemePreference> GetOrCreatePreferenceAsync(Guid userId)
        {
            var preference = await _context.UserThemePreferences
                .FirstOrDefaultAsync(p => p.UsrId == userId);

            if (preference != null)
                return preference;

            preference = CreateDefaultPreference(userId);
            _context.UserThemePreferences.Add(preference);
            return preference;
        }

        public async Task<bool> ResetToDefaultAsync(Guid userId)
        {
            try
            {
                var preference = await _context.UserThemePreferences
                    .FirstOrDefaultAsync(p => p.UsrId == userId);

                if (preference == null)
                {
                    preference = CreateDefaultPreference(userId);
                    _context.UserThemePreferences.Add(preference);
                }
                else
                {
                    preference.NavMenuBackgroundColor = ThemeDefaults.NavMenuBackgroundColor;
                    preference.NavMenuTextColor = ThemeDefaults.NavMenuTextColor;
                    preference.MainLayoutBackgroundColor = ThemeDefaults.MainLayoutBackgroundColor;
                    preference.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static UserThemePreference CreateDefaultPreference(Guid userId) => new()
        {
            PreferenceId = Guid.NewGuid(),
            UsrId = userId,
            NavMenuBackgroundColor = ThemeDefaults.NavMenuBackgroundColor,
            NavMenuTextColor = ThemeDefaults.NavMenuTextColor,
            MainLayoutBackgroundColor = ThemeDefaults.MainLayoutBackgroundColor,
            FontHeaderH6 = ThemeDefaults.FontHeaderH6,
            FontTieuDe = ThemeDefaults.FontTieuDe,
            FontNoiDung = ThemeDefaults.FontNoiDung,
            FontNoiDungLuoi = ThemeDefaults.FontNoiDungLuoi,
            FontNavMenu = ThemeDefaults.FontNavMenu,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
