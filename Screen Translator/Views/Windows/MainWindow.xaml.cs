using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Screen_Translator.Helpers;
using Screen_Translator.Service;
using Tesseract;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Window;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Screen_Translator.Views.Windows
{
    public partial class MainWindow : FluentWindow
    {
        public BitmapSource? ImageToScan = null;
        public SettingsWindow SettingsWindow;
        
        public MainWindow()
        {
            Watcher.Watch(this);
            InitializeComponent();
            App.LanguageChanged += OnLanguageChanged;
            App.LanguageChanged += UpdateMenuItems;
            OnLanguageChanged();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (Properties.Appearance.Default.MinimizeInsteadOfClosing)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Theme.Apply(ThemeTypeConverter.Convert(Properties.Appearance.Default.Theme));
            SettingsWindow = new(this);
            LocationChanged += (_, _) => SettingsWindow.CenterOnParent();
            SizeChanged += (_, _) => SettingsWindow.CenterOnParent();
            StateChanged += MainWindow_StateChanged;
            Activated += (_, _) => { if (SettingsWindow.IsVisible) SettingsWindow.Activate(); };

            if (Properties.Appearance.Default.StartMinimized)
                WindowState = WindowState.Minimized;
        }

        private void MainWindow_StateChanged(object? sender, EventArgs eventArgs)
        {
            SettingsWindow.CenterOnParent();
            if (WindowState == WindowState.Minimized)
                ShowInTaskbar = !Properties.Appearance.Default.MinimizeToTray;
        }

        private void OnLanguageChanged()
        {
            SourceComboBox.ItemsSource = null;
            OutputComboBox.ItemsSource = null;
            
            SourceComboBox.ItemsSource = App.TranslationLanguages.OrderBy(l => l.DisplayName);
            OutputComboBox.ItemsSource = App.TranslationLanguages.OrderBy(l => l.DisplayName);
            
            SourceComboBox.SelectedItem = App.TranslationLanguages.First(l => Equals(l, Properties.Translator.Default.Source));
            OutputComboBox.SelectedItem = App.TranslationLanguages.First(l => Equals(l, Properties.Translator.Default.Target));
        }
        
        private void UpdateMenuItems()
        {
            var openMenuItem = new MenuItem();
            openMenuItem.Header = FindResource("Open");
            openMenuItem.Click += OpenMenuItem_OnClick;

            var scanScreenMenuItem = new MenuItem();
            scanScreenMenuItem.Header = FindResource("ToolTipScanScreen");
            scanScreenMenuItem.Click += ScanScreenButton_OnClick;

            var exitMenuItem = new MenuItem();
            exitMenuItem.Header = FindResource("Exit");
            exitMenuItem.Click += ExitMenuItem_OnClick;
            
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(openMenuItem);
            contextMenu.Items.Add(scanScreenMenuItem);
            contextMenu.Items.Add(exitMenuItem);

            NotifyIcon.Menu = contextMenu;
        }


        private void SettingsButton_OnClick(object sender, RoutedEventArgs e) => SettingsWindow.Show();

        private async void TranslateButton_OnClick(object? sender = null, RoutedEventArgs? e = null)
        {
            TranslationProgressRing.Visibility = Visibility.Visible;
            var text = SourceTextBox.Text;
            var source = SourceComboBox.SelectedItem as CultureInfo;
            var target = OutputComboBox.SelectedItem as CultureInfo;
            try
            {
                var output = await Translator.Translate(text,
                    target!.TwoLetterISOLanguageName,
                    source!.TwoLetterISOLanguageName);
                OutputTextBox.Text = output;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                await Snackbar.ShowAsync(FindResource("NetworkErrorTitle").ToString()!, 
                    FindResource("NetworkErrorMessage").ToString()!,
                    SymbolRegular.WifiWarning24,
                    ControlAppearance.Danger);
            }
            catch (Exception)
            {
                await Snackbar.ShowAsync(FindResource("UnknownErrorTitle").ToString()!,
                    FindResource("UnknownErrorMessage").ToString()!, 
                    SymbolRegular.Warning28, 
                    ControlAppearance.Caution);
            }
            finally
            {
                TranslationProgressRing.Visibility = Visibility.Hidden;
            }
        }

        private void SourceTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CharactersCountLabel.Content = SourceTextBox.Text.Length != 0 ? $"{SourceTextBox.Text.Length} / 5000" : " ";
            CharactersCountLabel.Foreground = SourceTextBox.Text.Length > 5000 ? Brushes.Red : Brushes.Azure;
        }

        private void ScanScreenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var snippingWindow = new SnippingWindow(this);
            if (snippingWindow.ShowDialog() ?? false)
                ScanImage();
        }

        private void ScanImage()
        {
            SourceTextBox.IsReadOnly = true;
            TesseractProgressRing.Visibility = Visibility.Visible;

            var worker = new BackgroundWorker();
            worker.DoWork += WorkerDoWork;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            if (ImageToScan is not null)
                worker.RunWorkerAsync(ImageHelper.ConvertBitmapSourceToByteArray(ImageToScan));
        }

        private void WorkerDoWork(object? sender, DoWorkEventArgs e)
        {
            var image = (byte[])e.Argument!;
            using var engine = new TesseractEngine("./tessdata", 
                Properties.Tesseract.Default.Language.ThreeLetterISOLanguageName);
            using var img = Pix.LoadFromMemory(image);
            using var page = engine.Process(img);
            e.Result = page.GetText();
        }

        private void WorkerRunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error is null)
                SourceTextBox.Text = (string)e.Result!;
            else
                Snackbar.Show(FindResource("RecognitionErrorTitle").ToString()!,
                    FindResource("RecognitionErrorMessage").ToString()!,
                    SymbolRegular.Warning28,
                    ControlAppearance.Caution);
            SourceTextBox.IsReadOnly = false;
            TesseractProgressRing.Visibility = Visibility.Hidden;

            if (!Equals(Properties.Tesseract.Default.Language, Properties.Translator.Default.Source))
                SourceComboBox.SelectedItem = Properties.Tesseract.Default.Language;

            TranslateButton_OnClick();
        }

        private void SwapButton_OnClick(object sender, RoutedEventArgs e) =>
            (SourceComboBox.SelectedItem, OutputComboBox.SelectedItem) = (OutputComboBox.SelectedItem, SourceComboBox.SelectedItem);

        private void SourceComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Properties.Translator.Default.Source = SourceComboBox.SelectedItem as CultureInfo ?? 
                                                   Properties.Translator.Default.Source;

        private void OutputComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Properties.Translator.Default.Target = OutputComboBox.SelectedItem as CultureInfo ?? 
                                                   Properties.Translator.Default.Target;

        private void OpenMenuItem_OnClick(object sender, RoutedEventArgs e) => Show();
        private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}