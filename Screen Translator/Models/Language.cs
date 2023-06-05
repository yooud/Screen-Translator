namespace Screen_Translator.Models;

public class Language
{
    public string DisplayName { get; }
    public string Code { get; }
    public bool IsDownloaded { get; set; }

    // public string State { get; set; }

    public Language(string displayName, string code, bool isDownloaded = false)
    {
        DisplayName = displayName;
        Code = code;
        IsDownloaded = isDownloaded;
    }

    public override string ToString() => DisplayName;
}