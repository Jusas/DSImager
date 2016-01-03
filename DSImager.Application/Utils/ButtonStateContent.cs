using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DSImager.ViewModels.States;

namespace DSImager.Application.Utils
{
    class ButtonStateContent
    {
        public Dictionary<int, string> StateContentPairs { get; set; }

        public ButtonStateContent()
        {
            StateContentPairs = new Dictionary<int, string>();
        }
    }
}
