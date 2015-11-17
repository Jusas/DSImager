using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DSImager.Application.Utils
{
    public static class ListExtensions
    {
        public static DoubleCollection ToDoubleCollection(this List<double> list)
        {
            return new DoubleCollection(list);
        }
    }
}
