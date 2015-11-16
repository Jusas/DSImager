using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;

namespace DSImager.Core.Interfaces
{
    public delegate void CameraChosenHandler(string cameraName);

    public interface ICameraService
    {        
        event CameraChosenHandler OnCameraChosen;
        bool Initialized { get; }
        string LastError { get; }
        ICameraV2 Camera { get; }

        string ChooseDevice();
        bool Initialize(string deviceId);

    }
}
