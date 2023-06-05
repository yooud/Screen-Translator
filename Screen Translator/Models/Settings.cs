namespace Screen_Translator.Models;

public class Settings
{
    public string[] TranslationLanguages { get; set; }
    public string[] Tessdata { get; set; }
    public string SourceLanguageCode { get; set; }
    public string OutputLanguageCode { get; set; }
    public string Theme { get; set; }
    public string? CurrentLanguage { get; set; }
    public bool MinimizeToTray { get; set; }
    public bool StartMinimized { get; set; }
    public bool MinimizeInsteadOfClosing { get; set; }
    public bool Startup { get; set; }

    public Settings(string[] translationLanguages, string[] tessdata, string sourceLanguageCode, string outputLanguageCode)
    {
        TranslationLanguages = translationLanguages;
        Tessdata = tessdata;
        SourceLanguageCode = sourceLanguageCode;
        OutputLanguageCode = outputLanguageCode;
        Theme = "-1";
        CurrentLanguage = null;
    }
}