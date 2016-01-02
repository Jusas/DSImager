using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DSImager.Core.Models;

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
                //var pbuf = (int) _exposureBitmap.BackBuffer;
                //GCHandle pinnedExposureBuf = GCHandle.Alloc(exposure.Pixels8Bit, GCHandleType.Pinned);
                //IntPtr exposureBufPtr = pinnedExposureBuf.AddrOfPinnedObject();
                //_exposureBitmap.WritePixels(fullRect, exposureBufPtr, exposure.Pixels8Bit.Length, exposure.Width);
                //pinnedExposureBuf.Free();

                GCHandle pinnedExposureBuf = GCHandle.Alloc(exposure.Pixels8Bit, GCHandleType.Pinned);
                IntPtr exposureBufPtr = pinnedExposureBuf.AddrOfPinnedObject();
                CopyMemory(_exposureBitmap.BackBuffer, exposureBufPtr, (uint) exposure.Pixels8Bit.Length);

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
