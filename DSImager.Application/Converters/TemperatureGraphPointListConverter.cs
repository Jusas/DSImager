using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DSImager.Core.System;

namespace DSImager.Application.Converters
{
    class TemperatureGraphPointListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            /*
            var temperatures = value as IList<double>;
            if(temperatures == null)
                return new PointCollection();
            double i = 0;
            */
            //return new PointCollection(temperatures.Select(t => new Point(i++, t)));
            var temperatures = value as IList<XY>;
            return new PointCollection(temperatures.Select(t => new Point(t.X, t.Y)));
            /*return new PointCollection(new []
            {
                new Point(0, 15), 
                new Point(2, 15), 
                new Point(3, 15),
                new Point(4, 15),
                new Point(5, 15),
                new Point(6, 15),
                new Point(7, 12),
                new Point(8, 6),
                new Point(9, -2)
            });*/
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
