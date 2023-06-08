using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Screen_Translator.Properties;
using Screen_Translator.Service;
using Screen_Translator.Views.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Translator = Screen_Translator.Service.Translator;

namespace Screen_Translator;

public partial class App : Application
{
    public static CultureInfo[] LocalizationLanguages;
    public static CultureInfo[] TranslationLanguages;
    public static CultureInfo[] Tessdata;
    public static readonly List<CultureInfo> DownloadedLanguages = new ();
    public static event Action DownloadedLanguagesUpdated;
    public static event Action LanguageChanged;
    public static CultureInfo? Language
    {
        set
        {
            if (value is null || value.LCID == 127)
                value = new CultureInfo("en");
            
            if (Equals(value, CultureInfo.CurrentUICulture))
                return;

            if (!LocalizationLanguages.Contains(value))
                value = new CultureInfo("en");

            Appearance.Default.Language = value;
            CultureInfo.CurrentUICulture = value;

            var dict = new ResourceDictionary
            {
                Source = new Uri($"Resources/Localizations/{value}.xaml", UriKind.RelativeOrAbsolute)
            };

            var oldDict = Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.StartsWith("Resources/Localizations") is true);
            if (oldDict is not null)
                Current.Resources.MergedDictionaries[Current.Resources.MergedDictionaries.IndexOf(oldDict)] = dict;
            else
                Current.Resources.MergedDictionaries.Add(dict);
            LanguageChanged?.Invoke();
        }
    }

    public App()
    {
        Theme.Changed += ThemeOnChanged;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var localizations = Current.Resources["LocalizationLanguages"] as string[];
        List<CultureInfo> localizedCultures = new();
        if (localizations is not null)
            foreach (var local in localizations)
                localizedCultures.Add(new CultureInfo(local));
        LocalizationLanguages = localizedCultures.ToArray();
        
        Appearance.Default.Startup = StartupManager.IsStartupEnabled();

        if (Screen_Translator.Properties.Tesseract.Default.Languages is null || 
            Screen_Translator.Properties.Translator.Default.Languages is null)
            Task.Run(UpdateLanguages).Wait();

        List<CultureInfo> languages = new();

        foreach (var code in Screen_Translator.Properties.Translator.Default.Languages!)
            languages.Add(new(code));

        TranslationLanguages = languages.ToArray();

        languages.Clear();
        foreach (var code in Screen_Translator.Properties.Tesseract.Default.Languages!)
        {
            var language = new CultureInfo(code);
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata", $"{language.ThreeLetterISOLanguageName}.traineddata")))
                DownloadedLanguages.Add(language);
            languages.Add(language);
        }

        Tessdata = languages.ToArray();
        if (Screen_Translator.Properties.Tesseract.Default.Language is null)
            Screen_Translator.Properties.Tesseract.Default.Language = DownloadedLanguages[0];
        Language = Appearance.Default.Language;
        base.OnStartup(e);
    }

    private void UpdateLanguages()
    {
        try
        {
            Screen_Translator.Properties.Tesseract.Default.Languages = new StringCollection();
            Screen_Translator.Properties.Tesseract.Default.Languages.AddRange(Translator.GetTessdata().GetAwaiter().GetResult());
            Screen_Translator.Properties.Translator.Default.Languages = new StringCollection();
            Screen_Translator.Properties.Translator.Default.Languages.AddRange(Translator.GetLangauges().GetAwaiter().GetResult());
            Screen_Translator.Properties.Translator.Default.Source = new CultureInfo(Screen_Translator.Properties.Translator.Default.Languages[0]!);
            Screen_Translator.Properties.Translator.Default.Target = new CultureInfo(Screen_Translator.Properties.Translator.Default.Languages[1]!);
        }
        catch (System.Net.Http.HttpRequestException)
        {
            (Current.MainWindow as MainWindow)?.Snackbar.Show(FindResource("NetworkErrorTitle")?.ToString()!, 
                FindResource("NetworkErrorMessage")?.ToString()!,
                SymbolRegular.WifiWarning24,
                ControlAppearance.Danger);
        }
        catch (Exception)
        {
            (Current.MainWindow as MainWindow)?.Snackbar.Show(FindResource("UnknownErrorTitle")?.ToString()!,
                FindResource("UnknownErrorMessage")?.ToString()!, 
                SymbolRegular.Warning28, 
                ControlAppearance.Caution);
        }
    }

    private void ThemeOnChanged(ThemeType currentTheme, Color systemAccent)
    {
        if (Appearance.Default.Theme != -1)
        {
            var theme = (ThemeType)Appearance.Default.Theme;
            if (theme != currentTheme)
                Theme.Apply(theme);
        }
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        Screen_Translator.Properties.Translator.Default.Save();
        Screen_Translator.Properties.Tesseract.Default.Save();
        Screen_Translator.Properties.Appearance.Default.Save();
    }

    public static void UpdateDownloadedLanguages() => DownloadedLanguagesUpdated?.Invoke();
}
