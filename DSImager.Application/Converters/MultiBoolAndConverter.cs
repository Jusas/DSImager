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
    /// MultiConverter, returns true if all values-array objects match parameter-array objects when converted to boolean.
    /// </summary>
    class MultiBoolAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var bools = parameter as IEnumerable<bool>;


            bool[] boolArr;
            if(bools != null)
                boolArr = bools.ToArray();
            else
            {
                boolArr = new bool[values.Length];
                for (var i = 0; i < values.Length; i++)
                    boolArr[i] = true;
                
            }

            if (boolArr.Length != values.Length)
                return false;

            for(var i = 0; i < values.Length; i++)
            {
                bool v = false;
                bool isBool = bool.TryParse(values[i].ToString(), out v);
                if (!isBool)
                    return false;

                if (v != boolArr[i])
                    return false;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
