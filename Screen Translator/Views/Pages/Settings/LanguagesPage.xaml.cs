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
        App.LanguageChanged += OnLanguageChanged;
        App.DownloadedLanguagesUpdated += OnLanguageChanged;
        OnLanguageChanged();
    }

    private void OnLanguageChanged()
    {
        DetectableLanguageComboBox.ItemsSource = null;
        TessdataListView.ItemsSource = null;
        
        DetectableLanguageComboBox.ItemsSource = App.DownloadedLanguages.OrderBy(l => l.DisplayName);
        DetectableLanguageComboBox.SelectedItem = App.DownloadedLanguages.First(l => Equals(l, Properties.Tesseract.Default.Language));
        if (DetectableLanguageComboBox.Items.Count == 1 || DetectableLanguageComboBox.SelectedItem is null)
            DetectableLanguageComboBox.SelectedIndex = 0;
        else if (DetectableLanguageComboBox.Items.Count > 1)
            DetectableLanguageComboBox.SelectedItem = App.DownloadedLanguages.First(l => Equals(l, Properties.Tesseract.Default.Language));
        if (DetectableLanguageComboBox.SelectedItem is not null)
            Properties.Tesseract.Default.Language = DetectableLanguageComboBox.SelectedItem as CultureInfo;
        
        TessdataListView.ItemsSource = App.Tessdata.OrderBy(l => l.DisplayName);
    }

    private void OpenFolderButton_OnClick(object sender, RoutedEventArgs e) =>
        Process.Start("explorer.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"));

    private void DetectableLanguageComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var box = sender as ComboBox;
        if (box?.SelectedItem is CultureInfo language)
            Properties.Tesseract.Default.Language = language;
    }
}