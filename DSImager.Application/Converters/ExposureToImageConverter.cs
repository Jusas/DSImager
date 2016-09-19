using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DSImager.Core.Models;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace DSImager.Application.Converters
{
    public class ExposureToImageConverter : IValueConverter
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

        private static WriteableBitmap _exposureBitmap;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var exposure = value as Exposure;
            
            if (exposure != null)
            {
                if (_exposureBitmap == null ||
                    ((int)_exposureBitmap.Width != exposure.Width || (int)_exposureBitmap.Height != exposure.Height))
                {
                    _exposureBitmap = new WriteableBitmap(exposure.Width, exposure.Height, 96, 96, 
                        PixelFormats.Gray8, null);

                }
                var fullRect = new Int32Rect(0, 0, (int) _exposureBitmap.Width, (int) _exposureBitmap.Height);
                
                GCHandle pinnedExposureBuf = GCHandle.Alloc(exposure.Pixels8Bit, GCHandleType.Pinned);
                IntPtr exposureBufPtr = pinnedExposureBuf.AddrOfPinnedObject();

                for (int i = 0; i < exposure.Height; i++)
                {
                    int skip = i*_exposureBitmap.BackBufferStride;
                    int ppos = i*exposure.Width;
                    CopyMemory(_exposureBitmap.BackBuffer + skip, exposureBufPtr + ppos, (uint)exposure.Width);
                }
                
                _exposureBitmap.Lock();
                _exposureBitmap.AddDirtyRect(fullRect);
                _exposureBitmap.Unlock();
                
                return _exposureBitmap;
                
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
