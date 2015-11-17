using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DSImager.Application.Controls
{
    public class PreviewExposureSlider : Slider
    {

        public static readonly DependencyProperty IndexBoundValuesProperty = DependencyProperty.Register("IndexBoundValues",
            typeof(IList<double>), typeof(PreviewExposureSlider), new UIPropertyMetadata(new List<double>()));

        [Bindable(true)]
        public IList<double> IndexBoundValues
        {
            get { return (IList<double>)GetValue(IndexBoundValuesProperty); }
            set { SetValue(IndexBoundValuesProperty, value); }
        }

        private ToolTip _autoToolTip;
        private ToolTip AutoToolTip
        {
            get
            {
                if (_autoToolTip == null)
                {
                    // Get the tooltip, it's private but we need it...
                    FieldInfo field = typeof(Slider).GetField("_autoToolTip",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    _autoToolTip = field.GetValue(this) as ToolTip;                    
                }
                return _autoToolTip;
            }
        }

        private void SetAutoToolTip()
        {
            if (Value <= IndexBoundValues.Count - 1)
                AutoToolTip.Content = string.Format("{0}s", IndexBoundValues[(int)Value]);
        }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            SetAutoToolTip();
        }

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            SetAutoToolTip();
        }


    }
}
