namespace WinForms;

public partial class SettingsForm : Form
{
    public SettingsForm()
    {
        InitializeComponent();
    }

    private void SetWorkingStatus(string title)
    {
        Cursor = Cursors.WaitCursor;
        statusLabel.Text = title;
    }

    private void SetIdleStatus()
    {
        Cursor = Cursors.Default;
        statusLabel.Text = null;
    }

    private void SettingsForm_Load(object sender, EventArgs e)
    {
        LoadInstalledLanguages();
    }

    private void LoadInstalledLanguages()
    {
        SetWorkingStatus("Loading installed languages...");
        try
        {
            var languages = LanguageService.GetInstalledLanguages();
            itemsPanel.SuspendLayout();
            itemsPanel.Controls.Clear();
            foreach (var language in languages)
            {
                var item = new LanguageControl(language);
                item.StatusChanged += ItemStatusChanged;
                itemsPanel.Controls.Add(item);
            }
            UpdateLastEnabledState();
            itemsPanel.ResumeLayout();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading languages: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetIdleStatus();
        }
    }

    private void UpdateLastEnabledState()
    {
        var languages = new List<Language>();
        foreach (var item in itemsPanel.Controls)
            if (item is LanguageControl control && control.Language is not null)
                languages.Add(control.Language);

        var lastEnabled = languages.Count(x => x.Enabled) <= 1;
        foreach (var item in itemsPanel.Controls)
            if (item is LanguageControl control && control.Language is not null)
            {
                var newState = control.Language.Enabled && lastEnabled;
                control.SetIsLastEnabled(newState);
            }
    }

    private void ItemStatusChanged(Language language)
    {
        var action = language.Enabled ? "Activating" : "Deactivating";
        var status = $"{action} \"{language.Name}\" language";

        SetWorkingStatus(status);
        try
        {
            if (language.Enabled)
                LanguageService.EnableLanguage(language.Tag);
            else
                LanguageService.DisableLanguage(language.Tag);

            UpdateLastEnabledState();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error on changing a language activation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetIdleStatus();
        }
    }
}
