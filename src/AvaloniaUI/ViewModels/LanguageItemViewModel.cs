using CommunityToolkit.Mvvm.ComponentModel;
using Core;

namespace AvaloniaUI.ViewModels;

public partial class LanguageItemViewModel : ObservableObject
{
    private readonly LanguageDto _language;

    [ObservableProperty]
    public partial bool Enabled { get; set; }

    public string Name => _language.Name;
    public string Tag => _language.Tag;
    public string ShortCode => _language.ShortCode.ToUpper();
    public bool IsUserInterface => _language.UserInterface;

    public LanguageItemViewModel(LanguageDto language)
    {
        _language = language;
        Enabled = language.Enabled;
    }

    partial void OnEnabledChanged(bool value)
    {
        if (_language.Enabled == value)
            return;

        try
        {
            if (value)
                LanguageService.EnableLanguage(Tag);
            else
                LanguageService.DisableLanguage(Tag);

            _language.Enabled = value;
        }
        catch
        {
            Enabled = !value;
            throw;
        }

        OnPropertyChanged(nameof(Enabled));
    }
}