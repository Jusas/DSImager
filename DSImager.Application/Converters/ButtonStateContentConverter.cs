using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DSImager.Application.Utils;
using DSImager.ViewModels.States;

namespace DSImager.Application.Converters
{
    class ButtonStateContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stateContent = parameter as ButtonStateContent;
            if (stateContent != null)
            {
                var contentKey = (int)value;
                if (stateContent.StateContentPairs.ContainsKey(contentKey))
                {
                    return stateContent.StateContentPairs[contentKey];
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
