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
                // Create default preference if not exists
                preference = new UserThemePreference
                {
                    PreferenceId = Guid.NewGuid(),
                    UsrId = userId,
                    NavMenuBackgroundColor = ThemeDefaults.NavMenuBackgroundColor,
                    NavMenuTextColor = ThemeDefaults.NavMenuTextColor,
                    MainLayoutBackgroundColor = ThemeDefaults.MainLayoutBackgroundColor,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

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
                    preference = new UserThemePreference
                    {
                        PreferenceId = Guid.NewGuid(),
                        UsrId = userId,
                        NavMenuBackgroundColor = navMenuColor,
                        NavMenuTextColor = navMenuTextColor,
                        MainLayoutBackgroundColor = mainLayoutColor,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserThemePreferences.Add(preference);
                }
                else
                {
                    preference.NavMenuBackgroundColor = navMenuColor;
                    preference.NavMenuTextColor = navMenuTextColor;
                    preference.MainLayoutBackgroundColor = mainLayoutColor;
                    preference.UpdatedAt = DateTime.UtcNow;
                }

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating theme preference: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> ResetToDefaultAsync(Guid userId)
        {
            try
            {
                var preference = await _context.UserThemePreferences
                    .FirstOrDefaultAsync(p => p.UsrId == userId);

                if (preference == null)
                {
                    preference = new UserThemePreference
                    {
                        PreferenceId = Guid.NewGuid(),
                        UsrId = userId,
                        NavMenuBackgroundColor = ThemeDefaults.NavMenuBackgroundColor,
                        NavMenuTextColor = ThemeDefaults.NavMenuTextColor,
                        MainLayoutBackgroundColor = ThemeDefaults.MainLayoutBackgroundColor,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
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
    }
}
