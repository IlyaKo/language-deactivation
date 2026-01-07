namespace WinForms;

public partial class LanguageControl : UserControl
{
    public event Action<Language>? StatusChanged;

    public Language? Language { private set; get; }

    private bool _isLastEnabled;

    public LanguageControl()
    {
        InitializeComponent();
    }

    public LanguageControl(Language language) : this()
    {
        Language = language;

        nameLabel.Text = Language.Name;

        UpdateUI();
    }

    public void SetIsLastEnabled(bool value)
    {
        _isLastEnabled = value;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Language is null)
            return;

        enabledCheckBox.Checked = Language.Enabled;
        toggleButton.Text = Language.Enabled ? "Disable" : "Enable";
        toggleButton.Enabled = !_isLastEnabled && !Language.UserInterface;
    }

    private void toggleButton_Click(object sender, EventArgs e)
    {
        if (Language is null)
            return;

        Language.Enabled = !Language.Enabled;
        toggleButton.Enabled = false;

        StatusChanged?.Invoke(Language);

        UpdateUI();
    }
}
