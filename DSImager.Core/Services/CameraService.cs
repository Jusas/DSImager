﻿using System;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using DSImager.Core.Devices;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;

namespace DSImager.Core.Services
{
    public class CameraService : ICameraService
    {
        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private ASCOM.DriverAccess.Camera _camera;
        private ILogService _logService;

        public string LastError { get; set; }

        public ICameraV2 ConnectedCamera
        {
            get { return _camera; }
        }

        public event CameraChosenHandler OnCameraChosen;
        public event ExposureProgressChangedHandler OnExposureProgressChanged;
        public event ExposureCompletedHandler OnExposureCompleted;

        public bool Initialized
        {
            get { return _camera != null; }
        }

        private bool _isReadyForNewExposure = true;
        public bool IsReadyForNewExposure
        {
            get
            {
                return _isReadyForNewExposure;
            }
        }

        private Exposure _exposure;
        public Exposure LastExposure { get { return _exposure; } }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public CameraService(ILogService logService)
        {
            _logService = logService;
        }

        public string ChooseDevice()
        {
            var chooser = new ASCOM.Utilities.Chooser {DeviceType = "Camera"};
            var device = chooser.Choose();
            if (OnCameraChosen != null)
                OnCameraChosen(device);
            return device;
        }

        /// <summary>
        /// Instantiates an ASCOM camera driver.
        /// </summary>
        /// <param name="deviceId">The device identifier</param>
        public bool Initialize(string deviceId)
        {
            try
            {
                _camera = new ASCOM.DriverAccess.Camera(deviceId);
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Camera driver instantiated: " + deviceId));
            }
            catch (Exception e)
            {
                LastError = "Failed to instantiate the camera driver! Exception: " + e.Message;
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, LastError));
                return false;
            }
            try
            {
                _camera.Connected = true;
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Camera driver connected"));
            }
            catch (Exception e)
            {
                LastError = "Failed to connect to the camera! Exception: " + e.Message;
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, LastError));
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Starts exposuring.
        /// No camera parameters given, ie. the Camera parameters must have already been set.
        /// ImagingService should do that part - this method just starts the exposure and 
        /// runs an exposure cycle and handles the different states and such.
        /// </summary>
        /// <param name="duration">The exposure duration, in seconds</param>
        /// <param name="isDarkFrame">If the frame is a dark (calibration) frame, set to true</param>
        /// <returns>
        /// Did exposuring succeed or fail. Note: upon stopping exposure, 
        /// the return value is true (exposure data is still saved). Upon aborting, the value is false.
        /// </returns>
        public async Task<bool> StartExposure(double duration, bool isDarkFrame = false)
        {
            _isReadyForNewExposure = false;

            if (_camera.CameraState != CameraStates.cameraIdle)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Warning, 
                    "Camera is not in idle state, unable to start a new exposure."));
                return false;
            }


            var metadata = new ExposureMetaData()
            {
                BinX = _camera.BinX,
                BinY = _camera.BinY,
                ExposureTime = duration
            };
            _camera.StartExposure(duration, !isDarkFrame);


            var startTime = DateTime.Now;
            while (!_camera.ImageReady && (_camera.CameraState != CameraStates.cameraExposing ||
                    _camera.CameraState != CameraStates.cameraReading))
            {
                await Task.Delay(200);
                var currentDuration = DateTime.Now - startTime;
                if (OnExposureProgressChanged != null)
                    OnExposureProgressChanged(currentDuration.TotalSeconds, duration);
            }

            // Done, download the image.
            if (_camera.ImageReady)
            {
                int[,] imgArr = (int[,]) _camera.ImageArray;
                int imageW = imgArr.GetUpperBound(0) + 1;
                int imageH = imgArr.GetUpperBound(1) + 1;

                // Convert to single dimension array and store as last exposure.              
                int[] pixelArr = new int[imgArr.Length];
                Buffer.BlockCopy(imgArr, 0, pixelArr, 0, imgArr.Length);
                Exposure exposure = new Exposure(imageW, imageH, pixelArr);
                metadata.ExposureTime = _camera.LastExposureDuration;
                exposure.MetaData = metadata;
                _exposure = exposure;

                _isReadyForNewExposure = true;

                if (OnExposureCompleted != null)
                    OnExposureCompleted(true, exposure);

                return true;
            }
            // The exposure was aborted and image could not be retrieved.
            else
            {
                _isReadyForNewExposure = true;

                if (OnExposureCompleted != null)
                    OnExposureCompleted(false, null);

                return false;
            }
            

        }

        public void StopExposure()
        {
            if(!_camera.CanStopExposure)
                throw new NotImplementedException("Camera does not support stopping exposures");

            if (_camera.CameraState == CameraStates.cameraExposing)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Stopping exposure"));
                _camera.StopExposure();
            }
        }

        public void AbortExposure()
        {
            if (!_camera.CanAbortExposure)
                throw new NotImplementedException("Camera does not support aborting exposures");

            if (_camera.CameraState == CameraStates.cameraExposing)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Aborting exposure"));
                _camera.AbortExposure();
            }
        }

        #endregion


    }
}