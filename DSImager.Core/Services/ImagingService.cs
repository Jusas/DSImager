using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.System;
using DSImager.Core.Models;
using DSImager.Core.Utils;
using nom.tam.fits;
using nom.tam.util;

namespace DSImager.Core.Services
{
    /// <summary>
    /// Imaging Service has the responsibility of taking images
    /// using the connected camera.
    /// </summary>
    public class ImagingService : IImagingService
    {

        // TODO: is this supposed to be here? Images are taken in sessions, when are dark frames taken and shoud that setting be in the ImageSequence itself?
        public bool DarkFrameMode { get; set; }
        public bool IsSessionPaused { get; private set; }

        public event ImagingCompletedHandler OnImagingComplete;
        public event ImagingSessionStartedHandler OnImagingSessionStarted;
        public event ImagingSessionPausedHandler OnImagingSessionPaused;
        public event ImagingSessionResumedHandler OnImagingSessionResumed;
        public event ImagingSessionCompletedHandler OnImagingSessionCompleted;
        public event ImagingSequenceStartedHandler OnImageSequenceStarted;
        public event ImagingSequenceCompletedHandler OnImageSequenceCompleted;


        private ICameraService _cameraService;
        private ILogService _logService;
        private IImageIoService _imageIoService;
        private ISystemEnvironment _systemEnvironment;

        private ImagingSession _storedSession;

        public ImageSequence CurrentImageSequence { get; set; }

        public ImagingSession CurrentImagingSession { get; set; }

        public ExposureVisualSettings ExposureVisualProcessingSettings { get; set; }

        public ImagingService(ICameraService cameraService, ILogService logService,
            IImageIoService ioService, ISystemEnvironment systemEnvironment)
        {
            _cameraService = cameraService;
            _logService = logService;
            _imageIoService = ioService;
            _systemEnvironment = systemEnvironment;
            ExposureVisualProcessingSettings = new ExposureVisualSettings()
            {
                AutoStretch = true,
                StretchMin = 0,
                StretchMax = -1
            };
            DarkFrameMode = false;
            IsSessionPaused = false;
        }

        /*
         * When we take a photo:
         * - Check that the camera is in a proper state to take it
         *   - Abort / stop exposure (stop if it is supported; stop still downloads the image)
         *     - If stopped, do the things you normally would do, dl & save the image to buffers, save proper metadata, 
         *       notify listeners etc.
         *   - Abort not supported, must report a failure.
         * - Check for camera state
         *   - If it's not idle, wait for x seconds before timeouting. If timed out, report failure.
         * - Camera is in proper state now, we may proceed: 
         *   - Start the exposure
         *   - Start polling camera with ImageReady - start the polling at the end of exposure time.
         *   - When exposure should be finished and ImageReady returns true, start downloading of the image.
         *   -- NOTE: CameraService will handle most of the complex stuff inside it.
         *   - Once the image has been downloaded, it will be stored inside ImagingService's buffers
         *   
         * - ImagingService provides an interface for autostretching and can return the source image or a 8-bit representation
         *     with adjusted or unadjusted histogram
         * - ImagingService provides an interface to save images to disk
         * - ImagingService is a high level interface that handles taking series of images as well as single images
         * - 
         * */

        public async Task TakeSingleExposure(double duration, int binXY, Rect? areaRect)
        {

            // Set binning for the camera accordingly.
            _cameraService.Camera.BinX = (short)binXY;
            _cameraService.Camera.BinY = (short)binXY;

            // If Width or Height is 0, assume full area.
            
            var rect = Rect.Full;
            if (areaRect == null || areaRect.Value.Width == 0 || areaRect.Value.Height == 0)
            {
                rect.Width = _cameraService.Camera.CameraXSize;
                rect.Height = _cameraService.Camera.CameraYSize;
            }
            else
            {
                rect = areaRect.Value;
            }

            _cameraService.Camera.StartX = rect.X;
            _cameraService.Camera.StartY = rect.Y;
            _cameraService.Camera.NumX = rect.Width / binXY;
            _cameraService.Camera.NumY = rect.Height / binXY;

            _cameraService.OnExposureCompleted += OnExposureCompleted;
            
            // When taking a single shot we have no session.
            CurrentImagingSession = null;
            var result = await _cameraService.TakeExposure(duration, false);
            if (!result)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error,
                    "TakeExposure returned false; last exposure could not be taken/saved."));
            }

            _cameraService.OnExposureCompleted -= OnExposureCompleted;

        }
        

        public async Task RunImagingSession(ImagingSession session)
        {

            int rightBound = _cameraService.Camera.CameraXSize;
            int bottomBound = _cameraService.Camera.CameraYSize;
            if (session.AreaRect.X + session.AreaRect.Width > rightBound || session.AreaRect.X < 0)
                throw new ArgumentOutOfRangeException("areaRect", "Pixel X area out of camera pixel bounds");
            if (session.AreaRect.Y + session.AreaRect.Height > bottomBound || session.AreaRect.Y < 0)
                throw new ArgumentOutOfRangeException("areaRect", "Pixel Y area out of camera pixel bounds");

            _cameraService.OnExposureCompleted += OnExposureCompleted;
            CurrentImagingSession = session;

            if (OnImagingSessionStarted != null)
                OnImagingSessionStarted(session);


            for (; session.CurrentRepeatIndex < session.RepeatTimes; session.CurrentRepeatIndex++)
            {
                for (; session.CurrentImageSequenceIndex < session.ImageSequences.Count; session.CurrentImageSequenceIndex++)
                {
                    var sequence = session.ImageSequences[session.CurrentImageSequenceIndex];
                    CurrentImageSequence = sequence;

                    if(!sequence.Enabled)
                        continue;

                    if (OnImageSequenceStarted != null)
                        OnImageSequenceStarted(session, sequence);

                    // Set binning for the camera accordingly.
                    _cameraService.Camera.BinX = (short)sequence.BinXY;
                    _cameraService.Camera.BinY = (short)sequence.BinXY;

                    // If Width or Height is 0, assume full area.
                    var rect = session.AreaRect;
                    if (rect.Width == 0 || rect.Height == 0)
                    {
                        rect.Width = _cameraService.Camera.CameraXSize;
                        rect.Height = _cameraService.Camera.CameraYSize;
                    }

                    _cameraService.Camera.StartX = rect.X;
                    _cameraService.Camera.StartY = rect.Y;
                    _cameraService.Camera.NumX = rect.Width / sequence.BinXY;
                    _cameraService.Camera.NumY = rect.Height / sequence.BinXY;

                    for (; sequence.CurrentExposureIndex < sequence.NumExposures; sequence.CurrentExposureIndex++)
                    {
                        bool result = false;
                        try
                        {
                            result = await _cameraService.TakeExposure(sequence.ExposureDuration, DarkFrameMode);
                            if (!result)
                            {
                                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, 
                                    "TakeExposure returned false; last exposure could not be taken/saved."));                                
                            }

                            // Note: saving happens in OnExposureCompleted and only when the session has SaveOutput set to true.
                            // Note: we do not increment CurrentExposureIndex because most likely the exposure was incomplete.
                            if (IsSessionPaused)
                                break;
                        }
                        catch (Exception e)
                        {
                            _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, 
                                "Exception occured on RunImagingSession: " + e.Message));
                            _cameraService.OnExposureCompleted -= OnExposureCompleted;
                            // Pause, do not abort. Something happened that might be fixable.
                            PauseCurrentImagingOperation("Error occured. Check log entry for details. Imaging paused, session may still be continuable.", true);
                            break;
                        }
                    }

                    if (session.PauseAfterEachSequence &&
                        sequence.CurrentExposureIndex == sequence.NumExposures &&
                        session.CurrentImageSequenceIndex < session.ImageSequences.Count - 1)
                    {
                        PauseCurrentImagingOperation("Sequence completed.", false);
                        session.CurrentImageSequenceIndex++;

                        if (OnImageSequenceCompleted != null)
                            OnImageSequenceCompleted(session, sequence);

                        break;
                    }

                    if (IsSessionPaused)
                        break;

                    if (OnImageSequenceCompleted != null)
                        OnImageSequenceCompleted(session, sequence);
                }


                if (session.PauseAfterEachRepeat &&
                    session.CurrentImageSequenceIndex == session.ImageSequences.Count &&
                    session.CurrentRepeatIndex < session.RepeatTimes - 1)
                {
                    PauseCurrentImagingOperation("Session run completed.", false);
                    session.CurrentRepeatIndex++;
                    break;
                }

                if (IsSessionPaused)
                    break;

                // Repeat completed, reset the current indices for the next round.
                session.CurrentImageSequenceIndex = 0;
                foreach (var imageSequence in session.ImageSequences)
                    imageSequence.CurrentExposureIndex = 0;
                
            }

            _cameraService.OnExposureCompleted -= OnExposureCompleted;

            if (!IsSessionPaused)
            {
                CurrentImagingSession = null;
                if (OnImagingSessionCompleted != null)
                    OnImagingSessionCompleted(session, true, false);
            }
                
            
        }

        public void CancelCurrentImagingOperation()
        {
            // If we're exposuring, stop it if camera supports stopping, otherwise abort
            if (_cameraService.IsExposuring)
            {
                _cameraService.StopOrAbortExposure();
            }
        }

        /// <summary>
        /// todo reason Sequence completed
        /// </summary>
        public void PauseCurrentImagingOperation(string reason, bool error)
        {
            if (IsSessionPaused)
                return;

            // Store the session and set paused true.
            _storedSession = CurrentImagingSession;
            CurrentImagingSession = null;
            IsSessionPaused = true;

            // Stop any exposure in progress.
            if (_cameraService.IsExposuring)
            {
                _cameraService.StopOrAbortExposure();
            }

            if (OnImagingSessionPaused != null)
                OnImagingSessionPaused(_storedSession, reason, error);
        }

        public async Task ResumeStoredImagingOperation()
        {
            if (IsSessionPaused)
            {
                IsSessionPaused = false;
                if (OnImagingSessionResumed != null)
                    OnImagingSessionResumed(_storedSession);
                await RunImagingSession(_storedSession);
            }
        }

        public void CancelStoredImagingOperation()
        {
            var session = IsSessionPaused ? _storedSession : CurrentImagingSession;
            _storedSession = null;
            IsSessionPaused = false;
            
            CancelCurrentImagingOperation();
            CurrentImagingSession = null;

            if (OnImagingSessionCompleted != null)
                OnImagingSessionCompleted(session, false, true);

        }


        private void SaveExposureToDisk(Exposure exposure, string format)
        {

            var ff = _imageIoService.WritableFileFormats.Where(wff => wff.Id == format).FirstOrDefault();
            if(ff == null)
                throw new ArgumentException("File format '" + format + "' was invalid, no writer for that file format found.", "format");

            var writer = _imageIoService.GetImageWriter(ff);

            // TODO path from configuration service
            var fname = CurrentImagingSession.GenerateFilename(CurrentImageSequence);
            //var path = Path.Combine(Environment.GetFolderPath(
            //        Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "DSImager");

            var path = CurrentImagingSession.OutputDirectory;
            if (string.IsNullOrEmpty(path))
            {
                if (!string.IsNullOrEmpty(_systemEnvironment.UserPicturesDirectory))
                {
                    path = Path.Combine(_systemEnvironment.UserPicturesDirectory,
                        "DSImager-session-" + CurrentImagingSession.Name.ToFilenameString());
                }
                else
                {
                    path = Path.Combine(_systemEnvironment.UserHomeDirectory,
                        "DSImager-session-" + CurrentImagingSession.Name.ToFilenameString());
                }
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            var filename = Path.Combine(path, fname);
            writer.Save(exposure, filename);

        }

        private void OnExposureCompleted(bool successful, Exposure exposure)
        {
            if (!ExposureVisualProcessingSettings.AutoStretch)
            {
                var min = ExposureVisualProcessingSettings.StretchMin;
                var max = ExposureVisualProcessingSettings.StretchMax;
                // No auto stretch, make sure we don't have initial conditions 
                // (default min/max), if values are non-default then apply stretch.
                if (!(min == 0 && (max == -1 || max == exposure.MaxDepth)))
                {
                    if (max == -1)
                        max = exposure.MaxDepth;
                    exposure.SetStretch(min, max);
                }
            }
            else
            {
                exposure.SetStretch();
            }

            if (CurrentImagingSession != null && CurrentImagingSession.SaveOutput)
            {
                try
                {
                    SaveExposureToDisk(exposure, CurrentImageSequence.FileFormat);
                }
                catch (Exception e)
                {
                    _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, "Unable to save exposure to disk! Exception raised: " + e.Message));
                }
            }

            if (OnImagingComplete != null)
                OnImagingComplete(successful, exposure);
        }
    }
}
