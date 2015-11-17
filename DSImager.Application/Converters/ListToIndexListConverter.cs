using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSImager.Application.Converters
{
    class ListToIndexListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (typeof (IEnumerable).IsAssignableFrom(value.GetType()))
            {
                var e = value as IEnumerable;
                var enumerator = e.GetEnumerator();
                List<int> indexes = new List<int>();
                int ix = 0;
                while (enumerator.MoveNext())
                {
                    indexes.Add(ix);
                    ix++;
                }
                return indexes;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
