using System;
using System.Globalization;
using System.Windows.Data;

namespace Screen_Translator.Helpers;

public class CapitalizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
            return StringHelper.Capitalize(stringValue);
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}