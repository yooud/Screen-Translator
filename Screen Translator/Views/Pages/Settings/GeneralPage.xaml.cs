using System.Windows;
using System.Windows.Controls;
using Screen_Translator.Helpers;
using Screen_Translator.Service;
using Wpf.Ui.Appearance;
using ComboBox = System.Windows.Controls.ComboBox;

namespace Screen_Translator.Views.Pages.Settings;

public partial class GeneralPage : Page
{
    public GeneralPage()
    {
        InitializeComponent();
        foreach (ComboBoxItem item in ThemeCheckBox.Items)
            if (item.Tag.ToString() == App.Cache.Settings!.Theme)
                ThemeCheckBox.SelectedItem = item;
        
        MinimizeToTrayToggleSwitch.IsChecked = App.Cache.Settings!.MinimizeToTray;
        StartMinimizedToggleSwitch.IsChecked = App.Cache.Settings!.StartMinimized;
        MinimizeInsteadOfClosingToggleSwitch.IsChecked = App.Cache.Settings!.MinimizeInsteadOfClosing;
        StartupToggleSwitch.IsChecked = App.Cache.Settings!.Startup;
    }

    private void ThemeCheckBox_OnSelectionChanged(object sender, SelectionChangedEventArgs _)
    {
        var item = ((sender as ComboBox)?.SelectedItem as ComboBoxItem)!;
        App.Cache.Settings!.Theme = item.Tag.ToString()!;
        Theme.Apply(ThemeTypeConverter.Convert(App.Cache.Settings.Theme));
    }

    private void MinimizeToTrayToggleSwitch_OnStatusChanged(object sender, RoutedEventArgs e) =>
        App.Cache.Settings!.MinimizeToTray = MinimizeToTrayToggleSwitch.IsChecked ?? false;

    private void StartMinimizedToggleSwitch_OnStatusChanged(object sender, RoutedEventArgs e) =>
        App.Cache.Settings!.StartMinimized = StartMinimizedToggleSwitch.IsChecked?? false;

    private void MinimizeInsteadOfClosingToggleSwitch_OnStatusChanged(object sender, RoutedEventArgs e) => 
        App.Cache.Settings!.MinimizeInsteadOfClosing = MinimizeInsteadOfClosingToggleSwitch.IsChecked ?? false;

    private void StartupToggleSwitch_OnStatusChanged(object sender, RoutedEventArgs e)
    {
        App.Cache.Settings!.Startup = StartupToggleSwitch.IsChecked ?? false;
        if (App.Cache.Settings!.Startup)
            StartupManager.EnableStartup();
        else 
            StartupManager.DisableStartup();
    }
}