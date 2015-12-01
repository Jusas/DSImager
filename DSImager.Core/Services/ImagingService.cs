using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.System;

namespace DSImager.Core.Services
{
    /// <summary>
    /// Imaging Service has the responsibility of taking images
    /// using the connected camera.
    /// </summary>
    public class ImagingService : IImagingService
    {

        public bool DarkFrameMode { get; set; }

        private ICameraService _cameraService;


        public ImagingService(ICameraService cameraService)
        {
            _cameraService = cameraService;
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

        public async Task<bool> TakeExposure(double duration, int binX, int binY, Rect areaRect)
        {
            _cameraService.ConnectedCamera.BinX = (short)binX;
            _cameraService.ConnectedCamera.BinY = (short)binY;

            // Todo check that the area isn't out of bounds, especially with binning on
            int rightBound = _cameraService.ConnectedCamera.CameraXSize / binX;
            int bottomBound = _cameraService.ConnectedCamera.CameraYSize / binY;
            if(areaRect.X + areaRect.Width > rightBound || areaRect.X < 0)
                throw new ArgumentOutOfRangeException("areaRect", "Pixel X area out of camera pixel bounds");
            if (areaRect.Y + areaRect.Height > bottomBound || areaRect.Y < 0)
                throw new ArgumentOutOfRangeException("areaRect", "Pixel Y area out of camera pixel bounds");

            _cameraService.ConnectedCamera.StartX = areaRect.X;
            _cameraService.ConnectedCamera.StartY = areaRect.Y;
            _cameraService.ConnectedCamera.NumX = areaRect.Width;
            _cameraService.ConnectedCamera.NumY = areaRect.Height;

            var result = await _cameraService.StartExposure(duration, DarkFrameMode);
            return result;

        }
    }
}
