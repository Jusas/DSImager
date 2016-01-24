using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSImager.Application.Converters
{
    class StringDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (targetType == typeof (double) && value is string )
            //{
            return value.ToString();
            
            //}
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = 0;
            var didConvert = double.TryParse((string) value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out val);
            if (!didConvert)
            {
                didConvert = double.TryParse((string)value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out val);
                if (!didConvert)
                    return null;
            }
            return val;
        }
    }
}
