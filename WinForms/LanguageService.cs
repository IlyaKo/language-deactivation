using Microsoft.Win32;
using System.Globalization;

namespace WinForms;

public abstract class LanguageService
{
    public static IReadOnlyList<Language> GetInstalledLanguages()
    {
        var installedLanguages = new List<Language>();

        var activeTags = InputLanguage.InstalledInputLanguages
                                      .Cast<InputLanguage>()
                                      .Select(l => l.Culture.Name)
                                      .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\Packages";

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key != null)
            {
                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    // Microsoft - Windows - LanguageFeatures - Basic - [lang] - Package
                    if (subKeyName.StartsWith("Microsoft-Windows-LanguageFeatures-Basic-", StringComparison.OrdinalIgnoreCase) &&
                        subKeyName.Contains("-Package"))
                    {
                        var parts = subKeyName.Split('-');

                        // [Prefix]...[Basic]-[Tag]-Package...
                        var packageIndex = Array.FindIndex(parts, p => p.StartsWith("Package", StringComparison.OrdinalIgnoreCase));
                        if (packageIndex > 0)
                        {
                            var start = subKeyName.IndexOf("Basic-", StringComparison.OrdinalIgnoreCase) + 6;
                            var end = subKeyName.IndexOf("-Package", StringComparison.OrdinalIgnoreCase);
                            if (start > 5 && end > start)
                            {
                                var tag = subKeyName[start..end];
                                tags.Add(tag);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // No action on registry read failure
        }

        foreach (var tag in activeTags.Union(tags, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(tag);

                installedLanguages.Add(new Language
                {
                    Name = culture.DisplayName,
                    Tag = culture.Name,
                    Enabled = activeTags.Contains(tag),
                    UserInterface = culture.Name.Equals(CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase),
                });
            }
            catch
            {
                // Ignore invalid culture tags
            }
        }

        return installedLanguages.AsReadOnly();
    }

    public static void EnableLanguage(string targetTag)
    {
        var activeTags = InputLanguage.InstalledInputLanguages
                                      .Cast<InputLanguage>()
                                      .Select(l => l.Culture.Name)
                                      .ToList();

        if (activeTags.Contains(targetTag, StringComparer.OrdinalIgnoreCase))
            return;

        activeTags.Add(targetTag);

        SetWinUserLanguageList(activeTags);
    }

    public static void DisableLanguage(string targetTag)
    {
        if (InputLanguage.InstalledInputLanguages.Count <= 1)
            throw new InvalidOperationException("You should not remove the last active language");

        if (InputLanguage.CurrentInputLanguage.Culture.Name.Equals(targetTag, StringComparison.OrdinalIgnoreCase))
            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
                if (!language.Culture.Name.Equals(targetTag, StringComparison.OrdinalIgnoreCase))
                {
                    InputLanguage.CurrentInputLanguage = language;
                    break;
                }

        var activeTags = InputLanguage.InstalledInputLanguages
                                      .Cast<InputLanguage>()
                                      .Select(l => l.Culture.Name)
                                      .Where(tag => !tag.Equals(targetTag, StringComparison.OrdinalIgnoreCase))
                                      .ToList();

        SetWinUserLanguageList(activeTags);
    }

    private static void SetWinUserLanguageList(List<string> tags)
    {
        if (tags.Count == 0)
            return;

        var tagListString = string.Join(",", tags.Select(t => $"'{t}'"));
        var script = $"Set-WinUserLanguageList -LanguageList {tagListString} -Force";

        PowerShellService.RunScript(script);
    }
}
