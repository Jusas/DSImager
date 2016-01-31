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
    public delegate void ImagingSessionPausedHandler(ImagingSession session, string reason, bool error);
    public delegate void ImagingSequenceStartedHandler(ImagingSession session, ImageSequence sequence);
    public delegate void ImagingSequenceCompletedHandler(ImagingSession session, ImageSequence sequence);
    public delegate void ImagingSessionResumedHandler(ImagingSession session);

    public interface IImagingService
    {
        event ImagingCompletedHandler OnImagingComplete;
        event ImagingSessionStartedHandler OnImagingSessionStarted;
        event ImagingSessionPausedHandler OnImagingSessionPaused;
        event ImagingSessionResumedHandler OnImagingSessionResumed;
        event ImagingSessionCompletedHandler OnImagingSessionCompleted;
        event ImagingSequenceStartedHandler OnImageSequenceStarted;
        event ImagingSequenceCompletedHandler OnImageSequenceCompleted;

        bool DarkFrameMode { get; set; }
        bool IsSessionPaused { get; }
        ImageSequence CurrentImageSequence { get; }
        ImagingSession CurrentImagingSession { get; }
        ExposureVisualSettings ExposureVisualProcessingSettings { get; }
        Task TakeSingleExposure(double duration, int binXY, Rect? areaRect);
        Task RunImagingSession(ImagingSession session);
        void PauseCurrentImagingOperation(string reason, bool error);
        Task ResumeStoredImagingOperation();
        void CancelStoredImagingOperation();
        void CancelCurrentImagingOperation();
    }
}
