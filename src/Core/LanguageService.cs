using Microsoft.Win32;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Core;

[SupportedOSPlatform("windows")]
public abstract partial class LanguageService
{
    public static IReadOnlyList<LanguageDto> GetInstalledLanguages()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return [];

        var installedLanguages = new List<LanguageDto>();

        var activeTags = GetActiveLanguageTags();

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

                installedLanguages.Add(new LanguageDto
                {
                    Name = culture.DisplayName,
                    Tag = culture.Name,
                    ShortCode = culture.Name.Split('-')[0],
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
        var activeTags = GetActiveLanguageTags().ToList();
        if (activeTags.Contains(targetTag, StringComparer.OrdinalIgnoreCase))
            return;

        activeTags.Add(targetTag);

        SetWinUserLanguageList(activeTags);
    }

    public static void DisableLanguage(string targetTag)
    {
        var activeTags = GetActiveLanguageTags().ToList();
        if (activeTags.Count <= 1)
            throw new InvalidOperationException("You should not remove the last active language");

        activeTags.RemoveAll(tag => tag.Equals(targetTag, StringComparison.OrdinalIgnoreCase));

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

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[]? lpList);

    private static HashSet<string> GetActiveLanguageTags()
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int count = GetKeyboardLayoutList(0, null);

        if (count > 0)
        {
            IntPtr[] layouts = new IntPtr[count];
            GetKeyboardLayoutList(count, layouts);
            foreach (var layout in layouts)
            {
                int langId = (int)layout & 0xFFFF;
                try
                {
                    var culture = CultureInfo.GetCultureInfo(langId);
                    tags.Add(culture.Name);
                }
                catch
                {
                    // Ignore unknown locals
                }
            }
        }
        return tags;
    }
}
