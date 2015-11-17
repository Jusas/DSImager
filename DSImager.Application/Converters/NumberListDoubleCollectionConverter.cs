using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DSImager.Application.Converters
{
    class NumberListDoubleCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var srcType = value.GetType();
            if (typeof (IEnumerable<double>).IsAssignableFrom(srcType))
            {
                var enumerable = value as IEnumerable<double>;
                return new DoubleCollection(enumerable);
            }
            if (typeof(IEnumerable<int>).IsAssignableFrom(srcType))
            {
                var enumerable = value as IEnumerable<int>;
                var doubles = new List<double>();
                enumerable.ToList().ForEach(i => doubles.Add((double)i));
                return new DoubleCollection(doubles);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
