using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.System
{
    public struct Rect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public static Rect Full { get { return new Rect() {Height = 0, Width = 0, X = 0, Y = 0}; } }

    }
}
