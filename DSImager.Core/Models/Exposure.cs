using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Models
{
    public class Exposure
    {
        private int[] _pixels;

        public int[] Pixels { get { return _pixels; } }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ExposureMetaData MetaData { get; set; }

        public Exposure(int width, int height, int[] pixels)
        {
            _pixels = pixels;
            Width = width;
            Height = height;
        }

    }
}
