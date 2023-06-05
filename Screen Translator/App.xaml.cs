using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Screen_Translator.Models;
using Screen_Translator.Service;
using Wpf.Ui.Appearance;

namespace Screen_Translator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Cache Cache { get; } = new ();
        public static Language[] TranslationLanguage;
        public static Language[] Tessdata;

        protected override void OnStartup(StartupEventArgs e)
        {
            var path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var processName = Process.GetCurrentProcess().MainModule!.FileName;
            Cache.Load();
            Cache.Settings!.Startup = StartupManager.IsStartupEnabled();
            List<Language> languages = new();
            
            foreach (var code in Cache.Settings!.TranslationLanguages)
            {
                var name = new CultureInfo(code).DisplayName;
                languages.Add(new (Capitalize(name), code));
            }
            TranslationLanguage = languages.OrderBy(l => l.DisplayName).ToArray();
            
            languages.Clear();
            foreach (var code in Cache.Settings!.Tessdata)
            {
                var name = new CultureInfo(code).DisplayName;
                var language = new Language(Capitalize(name), code);
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata", $"{code}.traineddata")))
                    language.IsDownloaded = true;
                languages.Add(language);
            }
            Tessdata = languages.OrderBy(l => l.DisplayName).ToArray();
            if (Cache.Settings!.CurrentLanguage is null)
                Cache.Settings.CurrentLanguage = Tessdata.First(l => l.IsDownloaded).Code;

            Theme.Changed += ThemeOnChanged;
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Cache.Save();
        }

        private void ThemeOnChanged(ThemeType currentTheme, Color systemAccent)
        {
            if (Cache.Settings!.Theme != "-1")
            {
                var theme = (ThemeType)Convert.ToInt32(Cache.Settings.Theme);
                if (theme != currentTheme) 
                    Theme.Apply(theme);
            }
        }

        private string Capitalize(string str) => char.ToUpper(str[0]) + str[1..];
    }
}