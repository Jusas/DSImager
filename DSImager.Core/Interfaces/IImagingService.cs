using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.System;

namespace DSImager.Core.Interfaces
{
    public interface IImagingService
    {
        bool DarkFrameMode { get; set; }
        Task<bool> TakeExposure(double duration, int binX, int binY, Rect areaRect);
    }
}
