using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using DSImager.Core.Models;

namespace DSImager.Core.Interfaces
{
    public enum ExposurePhase
    {
        Exposuring,
        Downloading
    }

    public delegate void CameraChosenHandler(string cameraName);
    public delegate void ExposureProgressChangedHandler(double currentExposureDuration, double targetExposureDuration, ExposurePhase phase);
    public delegate void ExposureCompletedHandler(bool successful, Exposure exposure);

    public interface ICameraService
    {        
        event CameraChosenHandler OnCameraChosen;
        event ExposureProgressChangedHandler OnExposureProgressChanged;
        event ExposureCompletedHandler OnExposureCompleted;

        bool Initialized { get; }
        string LastError { get; }
        ICameraV2 ConnectedCamera { get; }
        bool IsExposuring { get; }
        Exposure LastExposure { get; }        

        string ChooseDevice();
        bool Initialize(string deviceId);
        Task<bool> StartExposure(double duration, bool isDarkFrame = false);
        void StopExposure();
        void AbortExposure();
        void StopOrAbortExposure();



    }
}
