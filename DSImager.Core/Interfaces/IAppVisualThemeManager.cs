using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{

    public delegate void AppVisualThemeChangedHandler(string oldTheme, string newTheme);

    public interface IAppVisualThemeManager
    {
        event AppVisualThemeChangedHandler OnThemeChanged;

        string StandardTheme { get; }
        string StandardAccent { get; }

        string GetCurrentTheme();
        void SetTheme(string theme, string accent);

    }
}
