using System.Globalization;
using System.Management.Automation;

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

        using var powerShell = PowerShell.Create();
        powerShell.AddScript("Get-InstalledLanguage");
        var results = powerShell.Invoke();
        powerShell.Commands.Clear();
        if (powerShell.HadErrors || results.Count == 0)
            ThrowScriptError(powerShell);

        var languages = results[0].BaseObject as IEnumerable<object>
                      ?? throw new InvalidOperationException("Failed to retrieve user language list.");

        var systemTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var language in languages)
        {
            var idProperty = language.GetType().GetField("LanguageId");
            if (idProperty?.GetValue(language) is string tag)
                systemTags.Add(tag);
        }

        var allTags = activeTags.Union(systemTags);
        foreach (var tag in allTags)
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

            }

        return installedLanguages.AsReadOnly();
    }

    public static void EnableLanguage(string targetTag)
    {
        Task.Delay(3000).Wait(); // Simulate a long-running operation
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

        using var powerShell = PowerShell.Create();
        powerShell.AddCommand("Get-WinUserLanguageList");
        var results = powerShell.Invoke();
        powerShell.Commands.Clear();
        if (powerShell.HadErrors || results.Count == 0)
            ThrowScriptError(powerShell);

        var languages = results[0].BaseObject as IEnumerable<object>
                      ?? throw new InvalidOperationException("Failed to retrieve user language list.");

        if (languages.Count() <= 1)
            throw new InvalidOperationException("You should not remove the last active language");

        List<string> keepTags = [];
        var foundLanguage = false;

        foreach (var language in languages)
        {
            var property = language.GetType().GetProperty("LanguageTag");
            if (property?.GetValue(language) is not string tag)
                continue;

            if (tag.Equals(targetTag, StringComparison.OrdinalIgnoreCase))
                foundLanguage = true;
            else
                keepTags.Add(tag);
        }

        if (!foundLanguage)
            return;

        var script = $"Set-WinUserLanguageList -LanguageList '{string.Join("','", keepTags)}' -Force";

        powerShell.AddScript(script);
        powerShell.Invoke();

        if (powerShell.HadErrors)
            ThrowScriptError(powerShell);
    }

    private static void ThrowScriptError(PowerShell ps)
    {
        var error = ps.Streams.Error.FirstOrDefault();
        throw new Exception($"PowerShell Error: {error?.Exception?.Message ?? "Unknown error"}");
    }
}
