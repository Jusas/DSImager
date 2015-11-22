using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSImager.Application.Converters
{
    /// <summary>
    /// Returns a list item when the given parameter is an index and the value is the list.
    /// </summary>
    class ListItemFromIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (typeof(IList).IsAssignableFrom(value.GetType()) && typeof(Int32).IsAssignableFrom(parameter.GetType()))
            {
                var list = value as IList;
                var index = (int)parameter;
                if (list.Count > index)
                {
                    return list[index].ToString();
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
