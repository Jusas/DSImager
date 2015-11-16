using System;
using ASCOM.DeviceInterface;
using DSImager.Core.Interfaces;

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

        public ICameraV2 Camera
        {
            get { return _ascomInterface; }
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
                _logService.LogMessage(this, LogEventCategory.Informational, "Camera driver instantiated: " + deviceId);
            }
            catch (Exception e)
            {
                LastError = "Failed to instantiate the camera driver! Exception: " + e.Message;
                _logService.LogMessage(this, LogEventCategory.Error, LastError);
                return false;
            }
            return true;
        }

        #endregion


    }
}
