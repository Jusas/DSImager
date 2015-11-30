using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSImager.Application.Converters
{
    class SelectedListIndexItemToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && typeof (IEnumerable).IsAssignableFrom(parameter.GetType()))
            {
                var items = value as IEnumerable;
                var idx = (int) value;
                var e = items.GetEnumerator();
                for (int i = 0; i < idx; i++)
                {
                    e.MoveNext();                    
                }
                var item = e.Current;
                return item.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
