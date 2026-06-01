using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;

namespace AvaloniaUI.ViewModels;

public partial class LanguageItemViewModel(LanguageDto language) : ObservableObject
{
    private readonly LanguageDto _language = language;

    [ObservableProperty]
    public partial bool Enabled { get; set; } = language.Enabled;

    public string Name => _language.Name;
    public string Tag => _language.Tag;
    public string ShortCode => _language.ShortCode.ToUpper();
    public bool IsUserInterface => _language.UserInterface;

    [RelayCommand]
    private void Toggle()
    {
        if (!IsUserInterface)
            Enabled = !Enabled;
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
