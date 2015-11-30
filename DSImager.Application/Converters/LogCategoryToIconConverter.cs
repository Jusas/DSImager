using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DSImager.Core.Interfaces;
using FontAwesome.WPF;

namespace DSImager.Application.Converters
{
    class LogCategoryToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (LogEventCategory) value;
            if (val == LogEventCategory.Informational)
            {
                return FontAwesomeIcon.InfoCircle;
            }
            if (val == LogEventCategory.Error)
            {
                return FontAwesomeIcon.ExclamationCircle;
            }
            if (val == LogEventCategory.Warning)
            {
                return FontAwesomeIcon.Warning;
            }
            if (val == LogEventCategory.Verbose)
            {
                return FontAwesomeIcon.List;
            }

            return FontAwesomeIcon.List;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
