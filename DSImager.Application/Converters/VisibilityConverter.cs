using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DSImager.Application.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {


            var targetValue = true;
            if (parameter != null)
            {
                if (parameter is bool)
                    targetValue = (bool) parameter;
                else
                {
                    bool paramVal;
                    if (bool.TryParse(parameter.ToString(), out paramVal))
                        targetValue = paramVal;
                }
            }

            bool sourceValue = false;

            if (value != null && value is bool)
            {
                sourceValue = (bool) value;
            }
            else
            {
                // any non-null value is true.
                if (value != null)
                {
                    sourceValue = true;
                }
            }

            return sourceValue == targetValue ? Visibility.Visible : Visibility.Hidden;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
