using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Screen_Translator.Models;

namespace Screen_Translator.Views.Pages.Settings;

public partial class LanguagesPage : Page
{
    public LanguagesPage()
    {
        InitializeComponent();
        TessdataListView.ItemsSource = App.Tessdata;
        App.Cache.UpdatedLanguages += CacheOnUpdatedLanguages;
        App.Cache.UpdateDownloadedLanguages();
    }

    private void CacheOnUpdatedLanguages()
    {
        DetectableLanguageComboBox.Items.Clear();
        foreach (var language in App.Tessdata.Where(l => l.IsDownloaded).OrderBy(l => l.DisplayName))
        {
            DetectableLanguageComboBox.Items.Add(language);
            if (App.Cache.Settings!.CurrentLanguage == language.Code)
                DetectableLanguageComboBox.SelectedItem = language;
        }

        if (DetectableLanguageComboBox.Items.Count == 1 || DetectableLanguageComboBox.SelectedItem is null)
        {
            DetectableLanguageComboBox.SelectedIndex = 0;
            App.Cache.Settings!.CurrentLanguage = (DetectableLanguageComboBox.SelectedItem as Language)!.Code;
        }
    }

    private void OpenFolderButton_OnClick(object sender, RoutedEventArgs e) => 
        Process.Start("explorer.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"));

    private void DetectableLanguageComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var box = sender as ComboBox;
        var language = box?.SelectedItem as Language;
        if (language is not null)
            App.Cache.Settings!.CurrentLanguage = language.Code;
    }

    private void AddLanguageButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}