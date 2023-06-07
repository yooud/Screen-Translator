using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Screen_Translator.Helpers;
using Screen_Translator.Service;
using Tesseract;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Window;

namespace Screen_Translator.Views.Windows
{
    public partial class MainWindow : FluentWindow
    {
        public BitmapSource ImageScan = null!;
        private SettingsWindow _settingsWindow;

        // TODO Fix screen scaling
        public MainWindow()
        {
            Watcher.Watch(this);
            InitializeComponent();
            Loaded += OnLoad;
            Closing += OnClosing;
            App.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (Properties.Appearance.Default.MinimizeInsteadOfClosing)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Theme.Apply(ThemeTypeConverter.Convert(Properties.Appearance.Default.Theme));
            _settingsWindow = new(this);
            LocationChanged += (_, _) => _settingsWindow.CenterOnParent();
            SizeChanged += (_, _) => _settingsWindow.CenterOnParent();
            StateChanged += MainWindow_StateChanged;
            Activated += (_, _) => { if (_settingsWindow.IsVisible) _settingsWindow.Activate(); };

            if (Properties.Appearance.Default.StartMinimized)
                WindowState = WindowState.Minimized;
        }

        private void MainWindow_StateChanged(object? sender, EventArgs eventArgs)
        {
            _settingsWindow.CenterOnParent();
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

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e) => _settingsWindow.Show();

        private async void TranslateButton_OnClick(object? sender = null, RoutedEventArgs? e = null)
        {
            var text = SourceTextBox.Text;
            var source = SourceComboBox.SelectedItem as CultureInfo;
            var target = OutputComboBox.SelectedItem as CultureInfo;

            var output = await Translator.Translate(text, target!.TwoLetterISOLanguageName, source!.TwoLetterISOLanguageName);
            OutputTextBox.Text = output;
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

        private byte[] ConvertBitmapSourceToByteArray(BitmapSource bitmapSource)
        {
            using var stream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(stream);
            var bytes = stream.ToArray();
            return bytes;
        }

        private void ScanImage()
        {
            SourceTextBox.IsReadOnly = true;
            ProgressRing.Visibility = Visibility.Visible;

            var worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                var image = (byte[])e.Argument!;
                using var engine = new TesseractEngine("./tessdata", Properties.Tesseract.Default.Language.ThreeLetterISOLanguageName);
                using var img = Pix.LoadFromMemory(image);
                using var page = engine.Process(img);
                e.Result = page.GetText();
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                SourceTextBox.Text = e.Error == null ? (string)e.Result! : e.Error.Message + "\n" + e.Error.StackTrace;
                SourceTextBox.IsReadOnly = false;
                ProgressRing.Visibility = Visibility.Hidden;
                TranslateButton_OnClick();
            };

            worker.RunWorkerAsync(ConvertBitmapSourceToByteArray(ImageScan));
        }

        private void SwapButton_OnClick(object sender, RoutedEventArgs e) =>
            (SourceComboBox.SelectedItem, OutputComboBox.SelectedItem) = (OutputComboBox.SelectedItem, SourceComboBox.SelectedItem);

        private void SourceComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Properties.Translator.Default.Source = SourceComboBox.SelectedItem as CultureInfo ?? Properties.Translator.Default.Source;

        private void OutputComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Properties.Translator.Default.Target = OutputComboBox.SelectedItem as CultureInfo ?? Properties.Translator.Default.Target;
    }
}