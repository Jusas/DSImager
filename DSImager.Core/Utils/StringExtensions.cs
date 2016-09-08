using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Utils
{
    public static class StringExtensions
    {
        public static string ToFilenameString(this string str)
        {
            string safe = str;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(c, '_');
            }
            safe = safe.Replace(' ', '_');
            return safe;
        }
    }
}
