using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSImager.Application.Converters
{
    /// <summary>
    /// MultiConverter, returns true if one or more values-array entry equals parameter-array entry with same index.
    /// </summary>
    class MultiBoolOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var bools = parameter as IEnumerable<bool>;
            if (bools == null)
                return false;

            var boolArr = bools.ToArray();
            if (boolArr.Length != values.Length)
                return false;

            for(var i = 0; i < values.Length; i++)
            {
                bool v = false;
                bool isBool = bool.TryParse(values[i].ToString(), out v);
                if (!isBool)
                    continue;

                if (v != boolArr[i])
                    return true;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
