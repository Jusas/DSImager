using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using DSImager.ViewModels.States;

namespace DSImager.Application.Converters
{
    class MultiVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool[] bools;
            try
            {
                 bools = (parameter as IEnumerable<bool>).ToArray();
            }
            catch (Exception)
            {
                bools = null;
            }

            if (bools == null)
            {
                try
                {
                    object[] objs = (parameter as IEnumerable<object>).ToArray();
                    bools = objs.Select(o => o is bool || o != null).ToArray();
                }
                catch (Exception)
                {
                    bools = null;
                }
                
            }
            
            bool[] boolArr;
            List<bool> tmpBools = new List<bool>();
            if (bools != null)
            {
                foreach (var p in bools)
                {
                    if(p == null)
                        tmpBools.Add(false);
                    else
                        tmpBools.Add((bool)p);
                }
                boolArr = tmpBools.ToArray();
            }
            else
            {
                boolArr = new bool[values.Length];
                for (var i = 0; i < values.Length; i++)
                    boolArr[i] = true;

            }

            if (boolArr.Length != values.Length)
                return Visibility.Collapsed;

            for (var i = 0; i < values.Length; i++)
            {
                bool v = false;
                try
                {
                    bool isBool = bool.TryParse(values[i].ToString(), out v);
                    if (!isBool)
                    {
                        if (values[i] != null) // accept valid objects as "true"
                            v = true;
                        else
                            return Visibility.Collapsed;

                        if (v != boolArr[i])
                            return Visibility.Collapsed;
                    }
                    else
                    {
                        if (v != boolArr[i])
                            return Visibility.Collapsed;
                    }
                }
                catch (Exception)
                {
                    if(values[i] == null)
                        return Visibility.Collapsed;
                }
                
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
