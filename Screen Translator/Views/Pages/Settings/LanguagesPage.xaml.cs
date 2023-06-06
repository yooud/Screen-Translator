using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Screen_Translator.Views.Pages.Settings;

public partial class LanguagesPage : Page
{
    public LanguagesPage()
    {
        InitializeComponent();
        TessdataListView.ItemsSource = App.Tessdata;
        App.LanguagesUpdated += OnUpdatedLanguages;
        App.UpdateDownloadedLanguages();
    }

    private void OnUpdatedLanguages()
    {
        DetectableLanguageComboBox.Items.Clear();
        foreach (var language in App.DownloadedLanguages.OrderBy(l => l.DisplayName))
        {
            DetectableLanguageComboBox.Items.Add(language);
            if (Properties.Tesseract.Default.Language.LCID == language.LCID)
                DetectableLanguageComboBox.SelectedItem = language;
        }

        if (DetectableLanguageComboBox.Items.Count == 1 || DetectableLanguageComboBox.SelectedItem is null)
        {
            DetectableLanguageComboBox.SelectedIndex = 0;
            Properties.Tesseract.Default.Language = DetectableLanguageComboBox.SelectedItem as CultureInfo;
        }
    }

    private void OpenFolderButton_OnClick(object sender, RoutedEventArgs e) =>
        Process.Start("explorer.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"));

    private void DetectableLanguageComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var box = sender as ComboBox;
        if (box?.SelectedItem is CultureInfo language)
            Properties.Tesseract.Default.Language = language;
    }

    private void AddLanguageButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}