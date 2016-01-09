using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.System;

namespace DSImager.Core.Models
{
    public class Exposure
    {

        public delegate void HistogramStretchChangedHandler();
        public event HistogramStretchChangedHandler OnHistogramStretchChanged;

        private int[] _pixels;

        public int[] Pixels { get { return _pixels; } }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int MaxDepth { get; private set; }

        private int _pixelMinValue = int.MaxValue;
        private int _pixelMaxValue = int.MinValue;

        private int _stretchMin = 0;
        public int StretchMin { get { return _stretchMin; } }
        private int _stretchMax = 0;
        public int StretchMax { get { return _stretchMax; } }

        public byte[] Pixels8Bit { get; private set; }
        public SortedDictionary<int, int> Histogram { get; private set; }

        public ExposureMetaData MetaData { get; set; }

        public Exposure(int width, int height, int[] pixels, int maxDepth, bool autoStretch)
        {
            _pixels = pixels;
            Width = width;
            Height = height;
            MaxDepth = maxDepth;

            CreateHistogram();
            Pixels8Bit = new byte[Width * Height];
            _stretchMin = autoStretch ? _pixelMinValue : 0;
            _stretchMax = autoStretch ? _pixelMaxValue : MaxDepth;
            CreateStretched8BitImageByteArray(_stretchMin, _stretchMax);
        }

        public void SetStretch(int stretchStart = -1, int stretchEnd = -1)
        {
            if (stretchStart == -1 && stretchEnd == -1)
            {
                _stretchMin = _pixelMinValue;
                _stretchMax = _pixelMaxValue;
                CreateStretched8BitImageByteArray(_pixelMinValue, _pixelMaxValue);
                if (OnHistogramStretchChanged != null)
                    OnHistogramStretchChanged();
                return;
            }

            if (_stretchMin == stretchStart && _stretchMax == stretchEnd)
                return;

            _stretchMin = stretchStart;
            _stretchMax = stretchEnd;
            CreateStretched8BitImageByteArray(stretchStart, stretchEnd);

            if (OnHistogramStretchChanged != null)
                OnHistogramStretchChanged();

        }
        
        private unsafe void CreateHistogram()
        {
            Histogram = new SortedDictionary<int, int>();
            int pixlen = _pixels.Length;
            fixed (int* pixels = _pixels)
            {
                int* pptr = pixels;
                for (int i = 0; i < pixlen; i++)
                {
                    int v = 0;
                    Histogram.TryGetValue(*pptr, out v);
                    v++;
                    Histogram[*pptr] = v;
                    if (*pptr < _pixelMinValue)
                        _pixelMinValue = *pptr;
                    if (*pptr > _pixelMaxValue)
                        _pixelMaxValue = *pptr;
                    pptr++;
                }
            }
        }

        private unsafe void CreateStretched8BitImageByteArray(int stretchStart, int stretchEnd)
        {
            int valuesPer8BitPixel = (MaxDepth+1) / 256;
            //int shifts = 0;
            //for (shifts = 0; valuesPer8BitPixel > 1; valuesPer8BitPixel = valuesPer8BitPixel >> 1, shifts++) ;
            int pixlen = _pixels.Length;
            fixed (int* pixels = _pixels)
            {
                fixed (byte* bytes = Pixels8Bit)
                {
                    int* pptr = pixels;
                    byte* bptr = bytes;
                    for (int i = 0; i < pixlen; i++)
                    {
                        /*var stretchedPixelValue = (*pptr - _pixelMinValue) / 
                            (_pixelMaxValue - _pixelMinValue) * MaxDepth;*/
                        var stretchedPixelValue = (int) ((*pptr - stretchStart) * (255.0 / (stretchEnd - stretchStart))); 
                            
                        //*bptr = (byte)(stretchedPixelValue >> shifts);
                        *bptr = (byte)(stretchedPixelValue);
                        bptr++;
                        pptr++;
                    }
                }
            }

        }
        
    }
}
