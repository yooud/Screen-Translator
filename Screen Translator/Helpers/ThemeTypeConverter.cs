using Wpf.Ui.Appearance;

namespace Screen_Translator.Helpers;

public static class ThemeTypeConverter
{
    public static ThemeType Convert(int theme) =>
        theme switch
        {
            -1 => Convert(Theme.GetSystemTheme()),
            1 => ThemeType.Dark,
            _ => ThemeType.Light
        };

    public static ThemeType Convert(SystemThemeType theme) =>
        theme switch
        {
            SystemThemeType.Dark => ThemeType.Dark,
            _ => ThemeType.Light
        };
}