﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.System;
using DSImager.Core.Models;

namespace DSImager.Core.Services
{
    /// <summary>
    /// Imaging Service has the responsibility of taking images
    /// using the connected camera.
    /// </summary>
    public class ImagingService : IImagingService
    {
        public List<ImageFormat> SupportedImageFormats { get; private set; }

        // TODO: is this supposed to be here? Images are taken in sessions, when are dark frames taken and shoud that setting be in the ImageSequence itself?
        public bool DarkFrameMode { get; set; }
        public bool IsSessionPaused { get; private set; }

        public event ImagingCompletedHandler OnImagingComplete;
        public event ImagingSessionStartedHandler OnImagingSessionStarted;
        public event ImagingSessionPausedHandler OnImagingSessionPaused;
        public event ImagingSessionPausedHandler OnImagingSessionResumed;
        public event ImagingSessionCompletedHandler OnImagingSessionCompleted;


        private ICameraService _cameraService;
        private ILogService _logService;

        private ImagingSession _storedSession;

        public ImageSequence CurrentImageSequence { get; set; }

        public ImagingSession CurrentImagingSession { get; set; }

        public ExposureVisualSettings ExposureVisualProcessingSettings { get; set; }

        public ImagingService(ICameraService cameraService, ILogService logService)
        {
            _cameraService = cameraService;
            _logService = logService;
            ExposureVisualProcessingSettings = new ExposureVisualSettings()
            {
                AutoStretch = true,
                StretchMin = 0,
                StretchMax = -1
            };
            SupportedImageFormats = new List<ImageFormat>()
            {
                ImageFormat.Fits, ImageFormat.Tiff
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
            var previewSequence = new ImageSequence();
            previewSequence.ExposureDuration = duration;
            previewSequence.BinXY = binXY;
            var sequences = new[] { previewSequence };
            if(areaRect == null)
                areaRect = new Rect {
                    Width = _cameraService.ConnectedCamera.CameraXSize,
                    Height = _cameraService.ConnectedCamera.CameraYSize,
                    X = 0,
                    Y = 0
                };

            var previewSession = new ImagingSession(sequences)
            {
                AreaRect = areaRect.Value,
                Name = "Preview"
            };

            await RunImagingSession(previewSession);
        }
        

        public async Task RunImagingSession(ImagingSession session)
        {

            int rightBound = _cameraService.ConnectedCamera.CameraXSize;
            int bottomBound = _cameraService.ConnectedCamera.CameraYSize;
            if (session.AreaRect.X + session.AreaRect.Width > rightBound || session.AreaRect.X < 0)
                throw new ArgumentOutOfRangeException("areaRect", "Pixel X area out of camera pixel bounds");
            if (session.AreaRect.Y + session.AreaRect.Height > bottomBound || session.AreaRect.Y < 0)
                throw new ArgumentOutOfRangeException("areaRect", "Pixel Y area out of camera pixel bounds");

            _cameraService.OnExposureCompleted += OnExposureCompleted;
            CurrentImagingSession = session;

            if (OnImagingSessionStarted != null)
                OnImagingSessionStarted(session);


            for (int r = session.CurrentRepeatIndex; r < session.RepeatTimes; r++)
            {
                for (int i = session.CurrentImageSequenceIndex; i < session.ImageSequences.Count; i++)
                {
                    var sequence = session.ImageSequences[i];
                    CurrentImageSequence = sequence;

                    // Set binning for the camera accordingly.
                    _cameraService.ConnectedCamera.BinX = (short)sequence.BinXY;
                    _cameraService.ConnectedCamera.BinY = (short)sequence.BinXY;

                    // If Width or Height is 0, assume full area.
                    var rect = session.AreaRect;
                    if (rect.Width == 0 || rect.Height == 0)
                    {
                        rect.Width = _cameraService.ConnectedCamera.CameraXSize;
                        rect.Height = _cameraService.ConnectedCamera.CameraYSize;
                    }

                    _cameraService.ConnectedCamera.StartX = rect.X;
                    _cameraService.ConnectedCamera.StartY = rect.Y;
                    _cameraService.ConnectedCamera.NumX = rect.Width / sequence.BinXY;
                    _cameraService.ConnectedCamera.NumY = rect.Height / sequence.BinXY;

                    for (int j = sequence.CurrentExposureIndex; j < sequence.NumExposures; j++)
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

                            // todo: save the exposure
                            // Note: we do not increment j because most likely the exposure was incomplete.
                            if (IsSessionPaused)
                                break;
                        }
                        catch (Exception e)
                        {
                            _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, 
                                "Exception occured on RunImagingSession: " + e.Message));
                            _cameraService.OnExposureCompleted -= OnExposureCompleted;
                            // Pause, do not abort. Something happened that might be fixable.
                            PauseCurrentImagingOperation();
                            break;
                        }
                    }

                    if (IsSessionPaused)
                        break;


                }

                if (IsSessionPaused)
                    break;

                session.CurrentRepeatIndex++;
            }

            _cameraService.OnExposureCompleted -= OnExposureCompleted;

            if (!IsSessionPaused)
            {
                // Hmm, what about preview shots, if we stop preview, shouldn't we set canceledByUser=true?
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
        /// 
        /// </summary>
        public void PauseCurrentImagingOperation()
        {
            if (IsSessionPaused)
                return;

            // Store the session and set paused true.
            _storedSession = CurrentImagingSession;
            IsSessionPaused = true;

            // Stop any exposure in progress.
            if (_cameraService.IsExposuring)
            {
                _cameraService.StopOrAbortExposure();
            }

            if (OnImagingSessionPaused != null)
                OnImagingSessionPaused(_storedSession);
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
            if (IsSessionPaused)
            {
                var session = _storedSession;
                _storedSession = null;
                IsSessionPaused = false;
                // Just in case
                CancelCurrentImagingOperation();

                if (OnImagingSessionCompleted != null)
                    OnImagingSessionCompleted(session, false, true);
            }
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
            
            if (OnImagingComplete != null)
                OnImagingComplete(successful, exposure);
        }
    }
}
