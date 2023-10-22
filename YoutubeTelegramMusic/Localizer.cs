using System.Globalization;
using System.Resources;
using YoutubeTelegramMusic.database;
using YoutubeTelegramMusic.Resources;

namespace YoutubeTelegramMusic;

public static class Localizer
{
    public static string GetValue(string key, Locale language)
    {
        string? ruValue = LanguageResources.ResourceManager.GetString(key, new CultureInfo("ru"));
        return language switch
        {
            Locale.EN => LanguageResources.ResourceManager.GetString(key) ?? key,
            Locale.RU => ruValue ?? LanguageResources.ResourceManager.GetString(key) ?? key,
            _ => LanguageResources.ResourceManager.GetString(key) ?? key
        };
    }
}