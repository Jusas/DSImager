using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSImager.Application.Converters
{
    /// <summary>
    /// Multiconverter, returns true if all values are interpreted as true.
    /// </summary>
    class BoolMultiAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var falseRegex = new Regex(@"false", RegexOptions.IgnoreCase);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                    return false;
                if (values[i] is int && (int) values[i] <= 0)
                    return false;
                if (values[i] is string && falseRegex.IsMatch(values[i].ToString()))
                    return false;
                if (values[i] is bool && (bool) values[i] == false)
                    return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
