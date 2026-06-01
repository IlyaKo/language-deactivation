namespace Core;

public sealed class LanguageDto
{
    public string Name { get; init; } = string.Empty;

    public string Tag { get; init; } = string.Empty;

    public string ShortCode { get; init; } = string.Empty;

    public bool Enabled { get; set; }

    public bool UserInterface { get; init; }
}
