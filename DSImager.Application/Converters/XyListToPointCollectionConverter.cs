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
    class XyListToPointCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var xyList = value as List<XY>;
            if (xyList == null)
                return null;

            var pointCollection = new PointCollection();
            foreach (var pt in xyList)
            {
                pointCollection.Add(new Point(pt.X, pt.Y));
            }
            return pointCollection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
