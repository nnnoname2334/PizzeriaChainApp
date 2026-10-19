using System;
using System.IO;
using System.Windows;

namespace PizzeriaChainApp.Helpers
{
    public static class ThemeManager
    {
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PizzeriaChainApp");
        private static readonly string ThemeFile = Path.Combine(SettingsDir, "theme.txt");

        private static ResourceDictionary _darkDictionary;

        public static bool IsDark { get; private set; }

        // Загрузка сохранённой темы при старте приложения
        public static void LoadSavedTheme()
        {
            bool dark = false;
            try
            {
                if (File.Exists(ThemeFile))
                    dark = File.ReadAllText(ThemeFile).Trim() == "Dark";
            }
            catch { }
            Apply(dark);
        }

        // Переключение темы по кнопке
        public static void Toggle()
        {
            Apply(!IsDark);
            Save();
        }

        private static void Apply(bool dark)
        {
            var merged = Application.Current.Resources.MergedDictionaries;

            if (_darkDictionary != null)
            {
                merged.Remove(_darkDictionary);
                _darkDictionary = null;
            }

            if (dark)
            {
                _darkDictionary = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/DarkTheme.xaml")
                };
                merged.Add(_darkDictionary);   // последний словарь переопределяет цвета
            }
            IsDark = dark;
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                File.WriteAllText(ThemeFile, IsDark ? "Dark" : "Light");
            }
            catch { }
        }
    }
}