using System.Management.Automation;

namespace WinForms;

public abstract class LanguageService
{
    public static IReadOnlyList<Language> GetInstalledLanguages()
    {
        var result = new List<Language>();

        foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
            result.Add(new Language { Name = language.Culture.DisplayName, Tag = language.Culture.Name, Enabled = true });

        return result.AsReadOnly();
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

        Task.Run(() =>
        {
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

        }).Wait();
    }

    private static void ThrowScriptError(PowerShell ps)
    {
        var error = ps.Streams.Error.FirstOrDefault();
        throw new Exception($"PowerShell Error: {error?.Exception?.Message ?? "Unknown error"}");
    }
}
