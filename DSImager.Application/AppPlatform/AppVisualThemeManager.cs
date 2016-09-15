using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using MahApps.Metro;

namespace DSImager.Application.AppPlatform
{
    public class AppVisualThemeManager : IAppVisualThemeManager
    {
        public string StandardTheme { get; private set; }
        public string StandardAccent { get; private set; }

        public AppVisualThemeManager()
        {
            StandardTheme = "BaseDark";
            StandardAccent = "Red";
        }

        public string GetCurrentTheme()
        {
            var themeAccentPair = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
            return themeAccentPair.Item1.Name;
        }

        public void SetTheme(string theme, string accent)
        {
            ThemeManager.ChangeAppStyle(System.Windows.Application.Current,
                        ThemeManager.GetAccent(accent), ThemeManager.GetAppTheme(theme));
        }
    }
}
