using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DSImager.Core.Models;

namespace DSImager.Application.Converters
{
    class ImageFileFormatStringConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var val = values[0] as string;
            var ffs = values[1] as IDictionary<string, string>;

            if (val == null || ffs == null)
                return null;

            if (ffs.ContainsKey(val))
                return ffs[val];

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
