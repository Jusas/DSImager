using System;
using DSImager.Core;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;

namespace DSImager.Core.Devices
{
    public class AscomCamera : ICamera
    {
        private ASCOM.DeviceInterface.ICameraV2 _camera;
        private ILogService _logService;

        private CameraInfo _information;
        public CameraInfo Information
        {
            get { return _information; }
        }

        private CameraCapabilities _capabilities;
        public CameraCapabilities Capabilities
        {
            get { return _capabilities; }
        }


        public AscomCamera(ASCOM.DeviceInterface.ICameraV2 camera)
        {
            _camera = camera;
            Initialize();
        }


        private void Initialize()
        {
            _information = new CameraInfo()
            {
                Description = _camera.Description,
                DriverInfo = _camera.DriverInfo,
                Name = _camera.Name,
                DriverVersion = _camera.DriverVersion,
                SensorName = _camera.SensorName,
                SensorType = _camera.SensorType.ToString()
            };

            _capabilities = new CameraCapabilities()
            {
                MaxBinX = _camera.MaxBinX,
                MaxBinY = _camera.MaxBinY,
                CanAbortExposure = _camera.CanAbortExposure,
                CanAsymmetricBin = _camera.CanAsymmetricBin,
                CanFastReadOut = _camera.CanFastReadout,
                CanGetCoolerPower = _camera.CanGetCoolerPower,
                CanPulseGuide = _camera.CanPulseGuide,
                CanSetCcdTemperature = _camera.CanSetCCDTemperature,
                HasShutter = _camera.HasShutter,                
                ElectronsPerAdu = _camera.ElectronsPerADU,
                MaxAdu = _camera.MaxADU,
                MaxExposure = _camera.ExposureMax,
                MinExposure = _camera.ExposureMin,
                PixelSizeX = _camera.PixelSizeX,
                PixelSizeY = _camera.PixelSizeY,
                ResolutionX = _camera.CameraXSize,
                ResolutionY = _camera.CameraYSize,
                Resolution = string.Format("{0} x {1}", _camera.CameraXSize, _camera.CameraYSize)
            };

            try { _capabilities.CanStopExposure = _camera.CanStopExposure; }
            catch (ASCOM.PropertyNotImplementedException e) { _capabilities.CanStopExposure = false; }

            try { _capabilities.HasAdjustableGain = _camera.Gain >= 0 || true; }
            catch (ASCOM.PropertyNotImplementedException e) { _capabilities.HasAdjustableGain = false; }

            try { _capabilities.MaxGain = _camera.GainMax.ToString(); }
            catch (ASCOM.PropertyNotImplementedException e) { _capabilities.MaxGain = "Not supported"; }

            try { _capabilities.MinGain = _camera.GainMin.ToString(); }
            catch (ASCOM.PropertyNotImplementedException e) { _capabilities.MinGain = "Not supported"; }

        }
        

    }
}
