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
    public delegate void ImagingSessionStartedHandler(ImagingSession session);
    public delegate void ImagingSessionCompletedHandler(ImagingSession session, bool completedSuccessfully, bool canceledByUser);
    public delegate void ImagingSessionPausedHandler(ImagingSession session);

    public enum ImageFormat
    {
        Tiff,
        Fits
    }

    public interface IImagingService
    {
        event ImagingCompletedHandler OnImagingComplete;
        event ImagingSessionStartedHandler OnImagingSessionStarted;
        event ImagingSessionPausedHandler OnImagingSessionPaused;
        event ImagingSessionPausedHandler OnImagingSessionResumed;
        event ImagingSessionCompletedHandler OnImagingSessionCompleted;

        bool DarkFrameMode { get; set; }
        bool IsSessionPaused { get; }
        List<ImageFormat> SupportedImageFormats { get; }
        ImageSequence CurrentImageSequence { get; }
        ImagingSession CurrentImagingSession { get; }
        ExposureVisualSettings ExposureVisualProcessingSettings { get; }
        Task TakeSingleExposure(double duration, int binXY, Rect? areaRect);
        Task RunImagingSession(ImagingSession session);
        void PauseCurrentImagingOperation();
        Task ResumeStoredImagingOperation();
        void CancelStoredImagingOperation();
        void CancelCurrentImagingOperation();
    }
}
