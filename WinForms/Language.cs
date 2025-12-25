namespace WinForms;

public sealed class Language
{
    public string Name { get; init; } = string.Empty;

    public string Tag { get; init; } = string.Empty;

    public bool Enabled { get; set; }

    public bool UserInterface { get; init; }
}
