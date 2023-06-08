using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Screen_Translator.Helpers;

public static class ImageHelper
{
    public static byte[] ConvertBitmapSourceToByteArray(BitmapSource bitmapSource)
    {
        if (bitmapSource is null)
            throw new ArgumentNullException(nameof(bitmapSource));

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

        using var memoryStream = new MemoryStream();
        encoder.Save(memoryStream);
        return memoryStream.ToArray();
    }
}
