using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace AvaloniaUI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<LanguageItemViewModel> Languages { get; set; } = [];

    public MainWindowViewModel()
    {
        LoadLanguages();
    }

    [RelayCommand]
    private void LoadLanguages()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        var installed = LanguageService.GetInstalledLanguages();
        Languages.Clear();

        var sorted = installed.OrderByDescending(x => x.UserInterface).ThenBy(x => x.Name);

        foreach (var lang in sorted)
            Languages.Add(new LanguageItemViewModel(lang));
    }
}
