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
        string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\Packages";

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
                        int packageIndex = Array.FindIndex(parts, p => p.StartsWith("Package", StringComparison.OrdinalIgnoreCase));
                        if (packageIndex > 0)
                        {
                            // Тег может состоять из двух частей (ru-ru) или одной? 
                            // В имени ключа он обычно идет как "ru-ru".
                            // Для надежности попробуем взять 2 части перед Package, если они похожи на тег.

                            // Вариант проще: вырезаем строку между "Basic-" и "-Package"
                            int start = subKeyName.IndexOf("Basic-", StringComparison.OrdinalIgnoreCase) + 6;
                            int end = subKeyName.IndexOf("-Package", StringComparison.OrdinalIgnoreCase);

                            if (start > 5 && end > start)
                            {
                                string tag = subKeyName.Substring(start, end - start);
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
        string hexLangId = (culture.LCID & 0xFFFF).ToString("x4");
        string layoutId = $"0000{hexLangId}";
        string layoutProfileString = $"{hexLangId}:{layoutId}";

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
        var targetLcid = culture.LCID;

        var count = WinApi.GetKeyboardLayoutList(0, null);
        var hklList = new IntPtr[count];
        WinApi.GetKeyboardLayoutList(count, hklList);

        if (hklList.Length <= 1)
            throw new InvalidOperationException("You should not remove the last active language");

        var found = false;
        foreach (var hkl in hklList)
        {
            var hklLcid = (int)(hkl.ToInt64() & 0xFFFF);
            if (hklLcid == targetLcid)
            {
                WinApi.UnloadKeyboardLayout(hkl);
                found = true;
                // Не делаем break, чтобы удалить все раскладки этого языка (если их несколько)
            }
        }

        if (!found)
        {
            // Если язык не найден в активных, ничего страшного — он уже отключен.
        }
    }
}
