using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Screen_Translator.Helpers;
using Screen_Translator.Properties;
using Screen_Translator.Service;
using Wpf.Ui.Appearance;
using ComboBox = System.Windows.Controls.ComboBox;
using CultureInfo = System.Globalization.CultureInfo;

namespace Screen_Translator.Views.Pages.Settings;

public partial class GeneralPage : Page
{
    public GeneralPage()
    {
        InitializeComponent();
        App.LanguageChanged += OnLanguageChanged;
        OnLanguageChanged();

        foreach (ComboBoxItem item in ThemeCheckBox.Items)
            if (Convert.ToInt32(item.Tag) == Appearance.Default.Theme)
                ThemeCheckBox.SelectedItem = item;

        MinimizeToTrayToggleSwitch.IsChecked = Appearance.Default.MinimizeToTray;
        StartMinimizedToggleSwitch.IsChecked = Appearance.Default.StartMinimized;
        MinimizeInsteadOfClosingToggleSwitch.IsChecked = Appearance.Default.MinimizeInsteadOfClosing;
        StartupToggleSwitch.IsChecked = Appearance.Default.Startup;
    }
    
    private void OnLanguageChanged()
    {
        LanguageComboBox.ItemsSource = null;
        LanguageComboBox.ItemsSource = App.LocalizationLanguages.OrderBy(l => l.DisplayName);
        LanguageComboBox.SelectedItem = App.LocalizationLanguages.First(l => Equals(l, Appearance.Default.Language));
    }

    private void ThemeCheckBox_OnSelectionChanged(object sender, SelectionChangedEventArgs _)
    {
        var item = (ThemeCheckBox.SelectedItem as ComboBoxItem)!;
        Appearance.Default.Theme = Convert.ToInt32(item.Tag);
        Theme.Apply(ThemeTypeConverter.Convert(Appearance.Default.Theme));
    }

    private void MinimizeToTrayToggleSwitch_OnStatusChanged(object sender, RoutedEventArgs e) =>
        Appearance.Default.MinimizeToTray = MinimizeToTrayToggleSwitch.IsChecked ?? false;

    private void StartMinimizedToggleSwitch_OnStatusChanged(object sender, RoutedEventArgs e) =>
        Appearance.Default.StartMinimized = StartMinimizedToggleSwitch.IsChecked ?? false;

    private void MinimizeInsteadOfClosingToggleSwitch_OnStatusChanged(object sender, RoutedEventArgs e) =>
        Appearance.Default.MinimizeInsteadOfClosing = MinimizeInsteadOfClosingToggleSwitch.IsChecked ?? false;

    private void StartupToggleSwitch_OnStatusChanged(object sender, RoutedEventArgs e)
    {
        Appearance.Default.Startup = StartupToggleSwitch.IsChecked ?? false;
        if (Appearance.Default.Startup)
            StartupManager.EnableStartup();
        else
            StartupManager.DisableStartup();
    }

    private void LanguageComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is CultureInfo language)
            App.Language = language;
    }
}