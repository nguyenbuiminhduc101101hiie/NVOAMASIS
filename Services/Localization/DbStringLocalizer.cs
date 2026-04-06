using System.Globalization;
using Microsoft.Extensions.Localization;

namespace NVOAMASIS.Services.Localization;

public class DbStringLocalizer : IStringLocalizer
{
    private readonly Dictionary<string, string> _resources;
    private readonly string _culture;
    private readonly bool _returnOnlyKeyIfNotFound;

    public DbStringLocalizer(Dictionary<string, string> resources, string culture, bool returnOnlyKeyIfNotFound = true)
    {
        _resources = resources ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _culture = culture;
        _returnOnlyKeyIfNotFound = returnOnlyKeyIfNotFound;
    }

    public LocalizedString this[string name]
    {
        get
        {
            if (string.IsNullOrEmpty(name))
            {
                return new LocalizedString(name, name, false);
            }

            if (_resources.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value))
            {
                return new LocalizedString(name, value, false);
            }

            return new LocalizedString(name, name, true);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var localizedString = this[name];
            
            if (!localizedString.ResourceNotFound && !string.IsNullOrEmpty(localizedString.Value))
            {
                try
                {
                    var formattedValue = string.Format(localizedString.Value, arguments);
                    return new LocalizedString(name, formattedValue, false);
                }
                catch
                {
                    return localizedString;
                }
            }

            return localizedString;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return _resources.Select(kvp => new LocalizedString(kvp.Key, kvp.Value, false));
    }

    public IStringLocalizer WithCulture(CultureInfo culture)
    {
        // This method is obsolete but still required by interface
        return this;
    }
}
