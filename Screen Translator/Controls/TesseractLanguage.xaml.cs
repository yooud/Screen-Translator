using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Screen_Translator.Service;
using Screen_Translator.Views.Windows;
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
        var snackbar = (Application.Current.MainWindow as MainWindow)?.SettingsWindow.Snackbar!;
        if (App.DownloadedLanguages.Contains(language))
        {
            App.DownloadedLanguages.Remove(language);
            App.UpdateDownloadedLanguages();
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata", $"{language.ThreeLetterISOLanguageName}.traineddata"));
            SetStyleButton(SymbolRegular.ArrowDownload48, ControlAppearance.Primary);
            snackbar.Timeout = 3500;
            var show = snackbar?.ShowAsync(FindResource("DeleteSuccessfulTitle").ToString()!,
                FindResource("DeleteSuccessfulMessage").ToString()!,
                SymbolRegular.CheckmarkCircle48,
                ControlAppearance.Success);
            if (show is not null)
                await show;
            snackbar!.Timeout = 10000;
        }
        else
        {
            SetStyleButton(SymbolRegular.ClockArrowDownload24, ControlAppearance.Info);
            try
            {
                await Translator.Download(language);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                var show = snackbar?.ShowAsync(FindResource("NetworkErrorTitle").ToString()!, 
                    FindResource("NetworkErrorMessage").ToString()!,
                    SymbolRegular.WifiWarning24,
                    ControlAppearance.Danger);
                if (show is not null)
                    await show;
            }
            catch (Exception)
            {
                var show = snackbar?.ShowAsync(FindResource("UnknownErrorTitle").ToString()!,
                    FindResource("UnknownErrorMessage").ToString()!,
                    SymbolRegular.Warning28,
                    ControlAppearance.Caution);
                if (show is not null)
                    await show;
            }

            if (App.DownloadedLanguages.Contains(language))
            {
                SetStyleButton(SymbolRegular.Delete48, ControlAppearance.Danger);
                snackbar.Timeout = 3500;
                var show = snackbar?.ShowAsync(FindResource("DownloadSuccessfulTitle").ToString()!,
                    FindResource("DownloadSuccessfulMessage").ToString()!,
                    SymbolRegular.CheckmarkCircle48,
                    ControlAppearance.Success);
                if (show is not null)
                    await show;
                snackbar!.Timeout = 10000;
            }
            else if (snackbar is not null)
            {
                SetStyleButton(SymbolRegular.ArrowDownload48, ControlAppearance.Primary);
            }
        }
    }

    private void SetStyleButton(SymbolRegular icon, ControlAppearance appearance)
    {
        DownloadButton.Icon = icon;
        DownloadButton.Appearance = appearance;
    }
}