using System;
using ASCOM.DeviceInterface;
using DSImager.Core.Interfaces;

namespace DSImager.ViewModels
{
    public class DeviceInfoViewModel : BaseViewModel<DeviceInfoViewModel>
    {
        private readonly ICameraService _cameraService;

        public ICameraV2 Camera { get { return _cameraService.Camera; } }
        

        // tabs: general, capabilities, exposure
        // general: name, description, driverinfo, driverversion, sensortype, sensorname
        // capabilities: camera size, bin size, can abort exposure, can asymmetricbin, can fastreadout, can getcoolerpower, can pulseguide,
        // can setccdtemp, can stopexposure, has shutter, has adjustable gain
        // exposure: exposuremin, exposuremax, maxadu, percentcompleted, pixelsize
        public DeviceInfoViewModel(ILogService logService, ICameraService cameraService)
            : base(logService)
        {
            _cameraService = cameraService;
        }

        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
        }

        public override void Initialize()
        {
        }
    }
}
