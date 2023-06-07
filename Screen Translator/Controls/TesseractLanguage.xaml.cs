using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Screen_Translator.Service;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace Screen_Translator.Controls;

public partial class TesseractLanguage : UserControl
{
    public TesseractLanguage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var language = (DataContext as CultureInfo)!;
        if (App.DownloadedLanguages.Contains(language))
        {
            DownloadButton.Icon = SymbolRegular.Delete48;
            DownloadButton.Appearance = ControlAppearance.Danger;
        }
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var language = (DataContext as CultureInfo)!;
        if (App.DownloadedLanguages.Contains(language))
        {
            App.DownloadedLanguages.Remove(language);
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata", $"{language.ThreeLetterISOLanguageName}.traineddata"));
            SetStyleButton(SymbolRegular.ArrowDownload48, ControlAppearance.Primary);
        }
        else
        {
            SetStyleButton(SymbolRegular.ClockArrowDownload24, ControlAppearance.Info);
            await Translator.Download(language); // TODO Catch network error
            if (App.DownloadedLanguages.Contains(language))
                SetStyleButton(SymbolRegular.Delete48, ControlAppearance.Danger);
            else
            {
                SetStyleButton(SymbolRegular.ArrowDownload48, ControlAppearance.Primary);
                // TODO Notify the user that the download has finished not successfully
            }
        }
    }

    private void SetStyleButton(SymbolRegular icon, ControlAppearance appearance)
    {
        DownloadButton.Icon = icon;
        DownloadButton.Appearance = appearance;
    }
}