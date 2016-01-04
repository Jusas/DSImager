using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Models;
using DSImager.Core.System;

namespace DSImager.Core.Interfaces
{

    public delegate void ImagingCompletedHandler(bool successful, Exposure exposure);

    public interface IImagingService
    {
        event ImagingCompletedHandler OnImagingComplete;

        bool DarkFrameMode { get; set; }
        ImageSequence CurrentImageSequence { get; set; }
        ImagingSession CurrentImagingSession { get; set; }
        Task<bool> TakeSingleExposure(double duration, int binX, int binY, Rect? areaRect);
        Task<bool> BeginImagingSession(ImagingSession session);
        void CancelCurrentImagingOperation();
    }
}
