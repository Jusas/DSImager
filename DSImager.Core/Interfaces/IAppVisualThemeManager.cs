using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public interface IAppVisualThemeManager
    {
        string StandardTheme { get; }
        string StandardAccent { get; }

        string GetCurrentTheme();
        void SetTheme(string theme, string accent);

    }
}
