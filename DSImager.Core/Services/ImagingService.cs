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
        private ICameraService _cameraService;

        public ImagingService(ICameraService cameraService)
        {
            _cameraService = cameraService;
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

        // todo device layer omaksi projektikseen

        public void TakeExposure(double duration, int binX, int binY, Rect areaRect)
        {
            
        }
    }
}
