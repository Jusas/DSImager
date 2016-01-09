using System;
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

        // TODO: is this supposed to be here? Images are taken in sessions, when are dark frames taken and shoud that setting be in the ImageSequence itself?
        public bool DarkFrameMode { get; set; }

        public event ImagingCompletedHandler OnImagingComplete;

        private ICameraService _cameraService;
        private ILogService _logService;

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
            DarkFrameMode = false; 
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

        public async Task<bool> TakeSingleExposure(double duration, int binX, int binY, Rect? areaRect)
        {
            CurrentImageSequence = new ImageSequence();
            CurrentImageSequence.ExposureDuration = duration;
            var sequences = new[] {CurrentImageSequence};
            if(areaRect == null)
                areaRect = new Rect {
                    Width = _cameraService.ConnectedCamera.CameraXSize,
                    Height = _cameraService.ConnectedCamera.CameraYSize,
                    X = 0,
                    Y = 0
                };

            CurrentImagingSession = new ImagingSession(sequences)
            {
                AreaRect = areaRect.Value,
                BinX = binX,
                BinY = binY,
                Name = "Preview"
            };

            return await BeginImagingSession(CurrentImagingSession);
        }
        

        public async Task<bool> BeginImagingSession(ImagingSession session)
        {
            // Set binning for the camera accordingly.
            _cameraService.ConnectedCamera.BinX = (short)session.BinX;
            _cameraService.ConnectedCamera.BinY = (short)session.BinY;

            int rightBound = _cameraService.ConnectedCamera.CameraXSize;
            int bottomBound = _cameraService.ConnectedCamera.CameraYSize;
            if (session.AreaRect.X + session.AreaRect.Width > rightBound || session.AreaRect.X < 0)
                throw new ArgumentOutOfRangeException("areaRect", "Pixel X area out of camera pixel bounds");
            if (session.AreaRect.Y + session.AreaRect.Height > bottomBound || session.AreaRect.Y < 0)
                throw new ArgumentOutOfRangeException("areaRect", "Pixel Y area out of camera pixel bounds");

            _cameraService.ConnectedCamera.StartX = session.AreaRect.X;
            _cameraService.ConnectedCamera.StartY = session.AreaRect.Y;
            _cameraService.ConnectedCamera.NumX = session.AreaRect.Width / session.BinX;
            _cameraService.ConnectedCamera.NumY = session.AreaRect.Height / session.BinY;

            _cameraService.OnExposureCompleted += OnExposureCompleted;

            // TODO: handling of several ImageSequences: now just runs the first frame of the first sequence.
            var sequence = session.ImageSequences.FirstOrDefault();
            bool result = false;
            try
            {
                result = await _cameraService.StartExposure(sequence.ExposureDuration, DarkFrameMode);
                _cameraService.OnExposureCompleted -= OnExposureCompleted;
            }
            catch (Exception e)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, "Exception occured on BeginImagingSession: " + e.Message));
                _cameraService.OnExposureCompleted -= OnExposureCompleted;
                throw;
            }

            return result;

            
        }

        public void CancelCurrentImagingOperation()
        {
            // If we're exposuring, stop it if camera supports stopping, otherwise abort
            if (_cameraService.IsExposuring)
            {
                _cameraService.StopOrAbortExposure();
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
