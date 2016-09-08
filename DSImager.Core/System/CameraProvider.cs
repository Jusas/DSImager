using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using DSImager.Core.Interfaces;

namespace DSImager.Core.System
{
    /// <summary>
    /// Actually ASCOM camera provider - should separate ASCOM specific implementations to
    /// another module later on (and abstract all device interfaces).
    /// </summary>
    public class CameraProvider : ICameraProvider
    {
        private static ICameraV2 _camera;

        public string ChooseCameraDeviceId()
        {
            var chooser = new ASCOM.Utilities.Chooser { DeviceType = "Camera" };
            var device = chooser.Choose();
            return device;
        }

        public ICameraV2 GetCamera(string deviceId)
        {
            _camera = _camera ?? new ASCOM.DriverAccess.Camera(deviceId);
            return _camera;
        }
    }
}
