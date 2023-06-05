using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Screen_Translator.Models;
using Screen_Translator.Service;
using Tesseract;
using System.Linq;
using Screen_Translator.Helpers;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Window;

namespace Screen_Translator.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public BitmapSource ImageScan = null!;
        private SettingsWindow _settingsWindow;
        
        // TODO Fix screen scaling
        public MainWindow()
        {
            Watcher.Watch(this);
            InitializeComponent();
            UpdateComboBoxes();
            Loaded += OnLoad;
            Closing += OnClosing;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (App.Cache.Settings!.MinimizeInsteadOfClosing)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;                
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Theme.Apply(ThemeTypeConverter.Convert(App.Cache.Settings?.Theme ?? "-1"));
            _settingsWindow = new(this);
            LocationChanged += (_, _) => _settingsWindow.CenterOnParent();
            SizeChanged += (_, _) => _settingsWindow.CenterOnParent();
            StateChanged += MainWindow_StateChanged;
            Activated += (_, _) => { if (_settingsWindow.IsVisible) _settingsWindow.Activate(); };
            
            if (App.Cache.Settings!.StartMinimized)
                WindowState = WindowState.Minimized;
        }

        private void MainWindow_StateChanged(object? sender, EventArgs eventArgs)
        {
            _settingsWindow.CenterOnParent();
            if (WindowState == WindowState.Minimized)
                ShowInTaskbar = !App.Cache.Settings!.MinimizeToTray;
        }

        private void UpdateComboBoxes()
        {
            SourceComboBox.ItemsSource = App.TranslationLanguage;            
            OutputComboBox.ItemsSource = App.TranslationLanguage;            
            
            SourceComboBox.SelectedItem = App.TranslationLanguage.First(l => l.Code == App.Cache.Settings!.SourceLanguageCode);
            OutputComboBox.SelectedItem = App.TranslationLanguage.First(l => l.Code == App.Cache.Settings!.OutputLanguageCode);
        }
        
        private void SettingsButton_OnClick(object sender, RoutedEventArgs e) => _settingsWindow.Show();
        
        private async void TranslateButton_OnClick(object? sender = null, RoutedEventArgs? e = null)
        {
            var sourceText = SourceTextBox.Text;
            var sourceLanguage = SourceComboBox.SelectedItem as Language;
            var outputLanguage = OutputComboBox.SelectedItem as Language;

            var output = await Translator.Translate(sourceText, outputLanguage!.Code, sourceLanguage!.Code);
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
                var image = (byte[]) e.Argument!;
                using var engine = new TesseractEngine("./tessdata", App.Cache.Settings!.CurrentLanguage, EngineMode.Default);
                using var img = Pix.LoadFromMemory(image);
                using var page = engine.Process(img);
                e.Result = page.GetText();
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                SourceTextBox.Text = e.Error == null ? (string) e.Result! : e.Error.Message + "\n" + e.Error.StackTrace;
                SourceTextBox.IsReadOnly = false;
                ProgressRing.Visibility = Visibility.Hidden;
                TranslateButton_OnClick();
            };
            
            worker.RunWorkerAsync(ConvertBitmapSourceToByteArray(ImageScan));
        }

        private void SwapButton_OnClick(object sender, RoutedEventArgs e) =>
            (SourceComboBox.SelectedItem, OutputComboBox.SelectedItem) = (OutputComboBox.SelectedItem, SourceComboBox.SelectedItem);

        private void SourceComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            App.Cache.Settings!.SourceLanguageCode = (SourceComboBox.SelectedItem as Language)!.Code;
        
        private void OutputComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            App.Cache.Settings!.OutputLanguageCode = (OutputComboBox.SelectedItem as Language)!.Code;
    }
}