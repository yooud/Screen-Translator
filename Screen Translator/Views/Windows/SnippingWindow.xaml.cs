using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Screen_Translator.Views.Windows
{
    public partial class SnippingWindow : Window
    {
        private System.Windows.Point _startPoint;
        private BitmapImage _imageBitmap = null!;
        private readonly MainWindow _owner;

        public SnippingWindow(MainWindow owner)
        {
            _owner = owner;
            InitializeComponent();
            KeyUp += SnippingWindow_KeyUp;

            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;

            Overlay_Left.Width = Width;
            Overlay_Left.Height = Height;
            GetImage();
        }

        private void SnippingWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) DialogResult = false;
        }

        private void GetImage()
        {
            var size = new Size((int)Width, (int)Height);
            var screenshot = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppRgb);
            using var g = Graphics.FromImage(screenshot);
            g.CopyFromScreen(new Point((int)Left, (int)Top), Point.Empty, size);

            using var stream = new System.IO.MemoryStream();
            screenshot.Save(stream, ImageFormat.Png);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            _imageBitmap = new BitmapImage();
            _imageBitmap.BeginInit();
            _imageBitmap.CacheOption = BitmapCacheOption.OnLoad;
            _imageBitmap.StreamSource = stream;
            _imageBitmap.EndInit();
            _imageBitmap.Freeze();

            Screenshot.Source = _imageBitmap;
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _startPoint = e.GetPosition(SnippingCanvas);

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var currentPosition = e.GetPosition(SnippingCanvas);
            var width = Math.Abs(currentPosition.X - _startPoint.X);
            var height = Math.Abs(currentPosition.Y - _startPoint.Y);
            var left = Math.Min(currentPosition.X, _startPoint.X);
            var top = Math.Min(currentPosition.Y, _startPoint.Y);

            SelectionRectangle.Width = width;
            SelectionRectangle.Height = height;
            Canvas.SetLeft(SelectionRectangle, left);
            Canvas.SetTop(SelectionRectangle, top);

            var leftRectWidth = left;
            var leftRectHeight = SnippingCanvas.ActualHeight;
            var leftRectTop = 0;
            var leftRectLeft = 0;

            var topRectWidth = width;
            var topRectHeight = top;
            var topRectTop = 0;
            var topRectLeft = left;

            var rightRectWidth = SnippingCanvas.ActualWidth - left - width;
            var rightRectHeight = SnippingCanvas.ActualHeight;
            var rightRectTop = 0;
            var rightRectLeft = left + width;

            var bottomRectWidth = width;
            var bottomRectHeight = SnippingCanvas.ActualHeight - top - height;
            var bottomRectTop = top + height;
            var bottomRectLeft = left;

            Overlay_Left.Width = leftRectWidth;
            Overlay_Left.Height = leftRectHeight;
            Canvas.SetLeft(Overlay_Left, leftRectLeft);
            Canvas.SetTop(Overlay_Left, leftRectTop);

            Overlay_Top.Width = topRectWidth;
            Overlay_Top.Height = topRectHeight;
            Canvas.SetLeft(Overlay_Top, topRectLeft);
            Canvas.SetTop(Overlay_Top, topRectTop);

            Overlay_Right.Width = rightRectWidth;
            Overlay_Right.Height = rightRectHeight;
            Canvas.SetLeft(Overlay_Right, rightRectLeft);
            Canvas.SetTop(Overlay_Right, rightRectTop);

            Overlay_Bottom.Width = bottomRectWidth;
            Overlay_Bottom.Height = bottomRectHeight;
            Canvas.SetLeft(Overlay_Bottom, bottomRectLeft);
            Canvas.SetTop(Overlay_Bottom, bottomRectTop);
        }

        private BitmapSource CaptureImage(FrameworkElement selectedRectangle)
        {
            var left = Canvas.GetLeft(SelectionRectangle);
            var top = Canvas.GetTop(SelectionRectangle);
            var width = SelectionRectangle.Width;
            var height = SelectionRectangle.Height;

            var renderTargetBitmap = new RenderTargetBitmap(
                (int)selectedRectangle.Width,
                (int)selectedRectangle.Height,
                96,
                96,
                PixelFormats.Pbgra32);

            renderTargetBitmap.Render(Screenshot);

            var croppedBitmap = new CroppedBitmap(
                _imageBitmap,
                new Int32Rect((int)left, (int)top, (int)width, (int)height));

            return croppedBitmap;
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectionRectangle.Width > 0 || SelectionRectangle.Height > 0)
            {
                _owner.ImageScan = CaptureImage(SelectionRectangle);
                DialogResult = true;
            }
            else
                DialogResult = false;
        }
    }
}