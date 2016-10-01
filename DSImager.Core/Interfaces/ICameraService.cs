using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public delegate void ExposureStartedHandler(double duration);
    public delegate void ExposureCompletedHandler(bool successful, Exposure exposure);

    public delegate void WarmUpStartedHandler();
    public delegate void WarmUpCompletedHandler();
    public delegate void WarmUpProgressChangedHandler(double targetDegrees, double currentDegrees);

    public interface ICameraService
    {        
        event CameraChosenHandler OnCameraChosen;
        event ExposureProgressChangedHandler OnExposureProgressChanged;
        event ExposureStartedHandler OnExposureStarted;
        event ExposureCompletedHandler OnExposureCompleted;
        event WarmUpStartedHandler OnWarmUpStarted;
        event WarmUpProgressChangedHandler OnWarmUpProgressChanged;
        event WarmUpCompletedHandler OnWarmUpCompleted;
        event WarmUpCompletedHandler OnWarmUpCanceled;

        bool Initialized { get; }
        string LastError { get; }
        ICameraV2 Camera { get; }
        bool IsExposuring { get; }
        Exposure LastExposure { get; }
        INotifyCollectionChanged CameraTemperatureUpdateNotifier { get; }
        double[] CameraTemperatureHistory { get; }
        double CurrentCCDTemperature { get; }
        double DesiredCCDTemperature { get; }
        double AmbientTemperature { get; }
        bool IsCoolerOn { get; }
        bool IsWarmingUp { get; }

        string ChooseDevice();
        bool Initialize(string deviceId);
        Task<bool> TakeExposure(double duration, bool isCalibrationFrame = false, bool garbageFrame = false);
        void StopExposure();
        void AbortExposure();
        void StopOrAbortExposure();

        void SetDesiredCCDTemperature(double degrees);
        void SetCoolerOn(bool on);
        void WarmUp();
        void CancelWarmUp();


        void UnInitialize();


    }
}
