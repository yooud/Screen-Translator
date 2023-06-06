using System;
using System.Windows;
using Microsoft.Win32;

namespace Screen_Translator.Service;

public static class StartupManager
{
    private const string Key = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

    public static bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(Key, true);
        return key?.GetValue(Application.ResourceAssembly.GetName().Name) is not null;
    }

    public static void EnableStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(Key, true);
        key?.SetValue(Application.ResourceAssembly.GetName().Name, Environment.ProcessPath!);
    }

    public static void DisableStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(Key, true);
        key?.DeleteValue(Application.ResourceAssembly.GetName().Name, false);
    }
}