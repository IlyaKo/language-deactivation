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

        // Путь к пакетам компонентов Windows
        // Примечание: Требуются права на чтение HKLM (обычно доступны пользователю)
        var keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\Packages";

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key != null)
            {
                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    // Ищем пакеты базового ввода: Microsoft-Windows-LanguageFeatures-Basic-[lang]-Package
                    if (subKeyName.StartsWith("Microsoft-Windows-LanguageFeatures-Basic-", StringComparison.OrdinalIgnoreCase) &&
                        subKeyName.Contains("-Package"))
                    {
                        // Формат имени: Microsoft-Windows-LanguageFeatures-Basic-ru-ru-Package~...
                        // Нам нужно извлечь "ru-ru"
                        var parts = subKeyName.Split('-');
                        // Обычно тег языка находится перед словом "Package". 
                        // Структура: [Prefix]...[Basic]-[Tag]-Package...

                        // Простой парсинг: ищем часть перед "Package"
                        var packageIndex = Array.FindIndex(parts, p => p.StartsWith("Package", StringComparison.OrdinalIgnoreCase));
                        if (packageIndex > 0)
                        {
                            // Тег может состоять из двух частей (ru-ru) или одной? 
                            // В имени ключа он обычно идет как "ru-ru".
                            // Для надежности попробуем взять 2 части перед Package, если они похожи на тег.

                            // Вариант проще: вырезаем строку между "Basic-" и "-Package"
                            var start = subKeyName.IndexOf("Basic-", StringComparison.OrdinalIgnoreCase) + 6;
                            var end = subKeyName.IndexOf("-Package", StringComparison.OrdinalIgnoreCase);

                            if (start > 5 && end > start)
                            {
                                var tag = subKeyName.Substring(start, end - start);
                                tags.Add(tag);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Если нет доступа к реестру или ключа не существует, возвращаем пустой список.
            // Приложение продолжит работать только с активными языками.
        }

        var allTags = activeTags.Union(tags, StringComparer.OrdinalIgnoreCase);

        foreach (var tag in allTags)
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
                // Игнорируем некорректные культуры
            }
        }

        return installedLanguages.AsReadOnly();
    }

    public static void EnableLanguage(string targetTag)
    {
        var culture = CultureInfo.GetCultureInfo(targetTag);
        var hexLangId = (culture.LCID & 0xFFFF).ToString("x4");
        var layoutId = $"0000{hexLangId}";
        var layoutProfileString = $"{hexLangId}:{layoutId}";

        try
        {
            WinApi.InstallLayoutOrTip(layoutProfileString, 0);
            WinApi.LoadKeyboardLayout(layoutId, WinApi.ActivationFlag | WinApi.SubstitutionOnFlag);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to enable language '{targetTag}': {ex.Message}");
        }
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

        var culture = CultureInfo.GetCultureInfo(targetTag);
        var hexLangId = (culture.LCID & 0xFFFF).ToString("x4");
        var layoutId = $"0000{hexLangId}";
        var layoutProfileString = $"{hexLangId}:{layoutId}";

        try
        {
            var result = WinApi.InstallLayoutOrTip(layoutProfileString, WinApi.DeactivationFlag);

            WinApi.PostMessage(WinApi.BroadcastFlag, WinApi.SettingChangeFlag, IntPtr.Zero, "intl");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to disable language '{targetTag}': {ex.Message}");
        }
    }
}
