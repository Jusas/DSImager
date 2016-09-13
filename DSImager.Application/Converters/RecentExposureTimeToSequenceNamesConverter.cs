using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSImager.Application.Converters
{
    public class RecentExposureTimeToSequenceNamesConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double exposure = (double)values[0];
            Dictionary<double, string> seqNamesForExposures = (Dictionary<double, string>)values[1];

            var name = "";
            if (seqNamesForExposures.ContainsKey(exposure))
                name = seqNamesForExposures[exposure];
            return string.IsNullOrEmpty(name) ? $"{exposure:F}" : $"{exposure:F} - {name}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
