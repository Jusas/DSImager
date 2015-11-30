using System;
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

        private ASCOM.DriverAccess.Camera _ascomInterface;
        private ILogService _logService;

        public string LastError { get; set; }

        private AscomCamera _camera;
        public ICamera ConnectedCamera
        {
            get { return _camera; }
        }

        public event CameraChosenHandler OnCameraChosen;

        public bool Initialized
        {
            get { return _ascomInterface != null; }
        }

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
                _ascomInterface = new ASCOM.DriverAccess.Camera(deviceId);
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
                _ascomInterface.Connected = true;
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Camera driver connected"));
                _camera = new AscomCamera(_ascomInterface);
            }
            catch (Exception e)
            {
                LastError = "Failed to connect to the camera! Exception: " + e.Message;
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, LastError));
                return false;
            }
            
            return true;
        }

        #endregion


    }
}
