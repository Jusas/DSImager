using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using DSImager.Core.Devices;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using DSImager.Core.Utils;

namespace DSImager.Core.Services
{
    public class CameraService : ICameraService
    {
        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        
        private ILogService _logService;
        private ICameraProvider _cameraProvider;

        public string LastError { get; set; }

        public ICameraV2 Camera { get; private set; }

        private IApplication _application;

        public event CameraChosenHandler OnCameraChosen;
        public event ExposureProgressChangedHandler OnExposureProgressChanged;
        public event ExposureCompletedHandler OnExposureCompleted;
        public event ExposureStartedHandler OnExposureStarted;
        public event WarmUpStartedHandler OnWarmUpStarted;
        public event WarmUpProgressChangedHandler OnWarmUpProgressChanged;
        public event WarmUpCompletedHandler OnWarmUpCompleted;
        public event WarmUpCompletedHandler OnWarmUpCanceled;

        public bool Initialized
        {
            get { return Camera != null; }
        }

        private bool _isExposuring = false;
        public bool IsExposuring
        {
            get
            {
                return _isExposuring;
            }
        }

        private Exposure _exposure;
        public Exposure LastExposure { get { return _exposure; } }

        const int StoredTemperatureHistoryLength = 36;
        const int TemperatureQueryInterval = 5;

        private CancellationTokenSource _temperatureMonitoringToken;
        private ObservableCollection<double> _cameraTemperatureHistory = new ObservableCollection<double>();
        public INotifyCollectionChanged CameraTemperatureUpdateNotifier
        {
            get
            {
                return _cameraTemperatureHistory;
            }
        }

        public double[] CameraTemperatureHistory
        {
            get { return _cameraTemperatureHistory.ToArray(); }
        }

        public double CurrentCCDTemperature
        {
            get
            {
                if (Camera != null && Camera.Connected && Camera.CanSetCCDTemperature)
                {
                    return Camera.CCDTemperature;
                }
                return Double.NaN;
            }
        }

        public double DesiredCCDTemperature
        {
            get
            {
                if (Camera != null && Camera.Connected && Camera.CanSetCCDTemperature)
                {
                    return Camera.SetCCDTemperature;
                }
                return Double.NaN;
            }
        }

        public double AmbientTemperature
        {
            get
            {
                if (Camera != null && Camera.Connected && Camera.CanSetCCDTemperature)
                {
                    try
                    {
                        return Camera.HeatSinkTemperature;
                    }
                    catch (Exception e)
                    {
                        return Double.NaN;
                    }
                    
                }
                return Double.NaN;
            }
        }

        /// <summary>
        /// Is the cooler on.
        /// </summary>
        public bool IsCoolerOn 
        {
            get
            {
                if (Camera != null && Camera.Connected && Camera.CanSetCCDTemperature)
                {
                    return Camera.CoolerOn;
                }
                return false;
            } 
        }

        private bool _isWarmingUp = false;
        /// <summary>
        /// Is the warmup sequence happening right now.
        /// </summary>
        public bool IsWarmingUp
        {
            get
            {
                return _isWarmingUp;
            }
        }

        private CancellationTokenSource _warmUpCancellationToken;

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public CameraService(ILogService logService, ICameraProvider cameraProvider, IApplication application)
        {
            _logService = logService;
            _application = application;
            _cameraProvider = cameraProvider;
        }

        public string ChooseDevice()
        {
            var device = _cameraProvider.ChooseCameraDeviceId();
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
                Camera = _cameraProvider.GetCamera(deviceId);
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
                Camera.Connected = true;
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Camera driver connected"));
                InitializeMonitoring();
            }
            catch (Exception e)
            {
                LastError = "Failed to connect to the camera! Exception: " + e.Message;
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, LastError));
                return false;
            }

            _application.OnAppExit += (sender, args) => UnInitialize();

            return true;
        }

        /// <summary>
        /// Uninitialize, ie. clean up.
        /// </summary>
        public void UnInitialize()
        {
            if (_temperatureMonitoringToken != null)
            {
                _temperatureMonitoringToken.Cancel();
            }
            if (Camera != null && Camera.Connected)
            {
                // Safeguards: set CCDTemperature to ambient temperature if it can be set.
                // Truthfully I'm not sure if this has effect after the camera gets disconnected.
                /*if (Camera.CanSetCCDTemperature)
                {
                    Camera.SetCCDTemperature = Double.IsNaN(Camera.HeatSinkTemperature) ? 0 : Camera.HeatSinkTemperature;
                }*/

                if (Camera.CameraState != CameraStates.cameraIdle)
                {
                    StopOrAbortExposure();
                }

                Camera.Connected = false;
            }
        }

        /// <summary>
        /// Starts exposuring.
        /// No camera parameters given, ie. the Camera parameters must have already been set.
        /// ImagingService should do that part - this method just starts the exposure and 
        /// runs an exposure cycle and handles the different states and such.
        /// </summary>
        /// <param name="duration">The exposure duration, in seconds</param>
        /// <param name="isCalibrationFrame">If the frame is a calibration frame, set to true</param>
        /// <returns>
        /// Did exposuring succeed or fail. Note: upon stopping exposure, 
        /// the return value is true (exposure data is still saved). Upon aborting, the value is false.
        /// </returns>
        public async Task<bool> TakeExposure(double duration, bool isCalibrationFrame = false)
        {
            _isExposuring = true;

            _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational,
                    string.Format("Starting new exposure: {0:F}s", duration)));

            if (Camera.CameraState != CameraStates.cameraIdle)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Warning, 
                    "Camera is not in idle state, unable to start a new exposure."));
                return false;
            }


            var metadata = new ExposureMetaData()
            {
                BinX = Camera.BinX,
                BinY = Camera.BinY,
                ExposureTime = duration
            };

            if (OnExposureStarted != null)
                OnExposureStarted(duration);

            Camera.StartExposure(duration, !isCalibrationFrame);
            
            if (OnExposureProgressChanged != null)
                OnExposureProgressChanged(0, duration, ExposurePhase.Exposuring);

            var startTime = DateTime.Now;
            TimeSpan currentDuration = TimeSpan.Zero;
            while (!Camera.ImageReady && (Camera.CameraState != CameraStates.cameraExposing ||
                    Camera.CameraState != CameraStates.cameraReading))
            {
                await Task.Delay(200);
                currentDuration = DateTime.Now - startTime;
                if (OnExposureProgressChanged != null)
                    OnExposureProgressChanged(currentDuration.TotalSeconds, duration, ExposurePhase.Exposuring);
            }

            // Done, download the image.
            if (Camera.ImageReady)
            {
                if (OnExposureProgressChanged != null)
                    OnExposureProgressChanged(currentDuration.TotalSeconds, duration, ExposurePhase.Downloading);

                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational,
                    "Exposure ready, downloading..."));

                int[,] imgArr = (int[,])Camera.ImageArray;
                int imageW = imgArr.GetLength(0);
                int imageH = imgArr.GetLength(1);

                // BlockCopy doesn't work here because the output row/column order is wrong.
                // We really want the data as a single dimensional array and this is not
                // the most efficient way to do it but will have to suffice until
                // I find a more efficient way.
                int[] pixelArr = new int[imgArr.GetLength(0) * imgArr.GetLength(1)];
                for (int y = 0; y < imageH; y++)
                {
                    for (int x = 0; x < imageW; x++)
                    {
                        pixelArr[y * imageW + x] = imgArr[x, y];
                    }
                }
                

                Exposure exposure = new Exposure(imageW, imageH, pixelArr, Camera.MaxADU, false);
                metadata.ExposureTime = Camera.LastExposureDuration;
                metadata.ExposureEndTime = DateTime.Now;
                exposure.MetaData = metadata;
                _exposure = exposure;

                _isExposuring = false;

                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational,
                    "Exposure done."));

                if (OnExposureCompleted != null)
                    OnExposureCompleted(true, exposure);

                return true;
            }
            // The exposure was aborted and image could not be retrieved.
            else
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Warning,
                    "Exposure done but image not ready, exposure must have been aborted."));

                _isExposuring = false;

                if (OnExposureCompleted != null)
                    OnExposureCompleted(false, null);

                return false;
            }
            

        }

        public void StopOrAbortExposure()
        {
            if (Camera.CanStopExposure)
            {
                StopExposure();
            }
            else
            {
                AbortExposure();
            }
        }

        public void StopExposure()
        {
            if(!Camera.CanStopExposure)
                throw new NotImplementedException("Camera does not support stopping exposures");

            if (Camera.CameraState == CameraStates.cameraExposing)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Stopping exposure"));
                Camera.StopExposure();
            }
        }

        public void AbortExposure()
        {
            if (!Camera.CanAbortExposure)
                throw new NotImplementedException("Camera does not support aborting exposures");

            if (Camera.CameraState == CameraStates.cameraExposing)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Aborting exposure"));
                Camera.AbortExposure();
            }
        }


        public void SetDesiredCCDTemperature(double degrees)
        {
            if (Camera != null && Camera.Connected && Camera.CanSetCCDTemperature)
            {
                // Just a sanity check here.
                int saneMin = -100;
                int saneMax = 70;

                if (degrees < saneMin || degrees > saneMax)
                {
                    var d = degrees;
                    degrees = Math.Max(Math.Min(degrees, saneMax), saneMin);
                    _logService.LogMessage(new LogMessage(this, LogEventCategory.Warning, string.Format(
                        "Tried to set CCD temperature to a potentially nonsensical value ({0}C). Limiting it to {1}C.",
                        d, degrees)));                    
                }
                Camera.SetCCDTemperature = degrees;                
            }
        }

        public void SetCoolerOn(bool on)
        {
            if (Camera != null && Camera.Connected && Camera.CanSetCCDTemperature && Camera.CoolerOn != on)
            {
                Camera.CoolerOn = on;
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, 
                    "Turned CCD cooler " + (on ? "on" : "off")));
            }
        }

        public void WarmUp()
        {            
            if (IsWarmingUp)
                return;

            if (Camera == null || !Camera.Connected || !Camera.CanSetCCDTemperature || !Camera.CoolerOn)
                return;

            _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, 
                "Starting CCD warmup to reach ambient temperature."));
            RunWarmUp();
        }

        public void CancelWarmUp()
        {
            if (_warmUpCancellationToken != null && !_warmUpCancellationToken.IsCancellationRequested)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Canceling warmup task."));
                _warmUpCancellationToken.Cancel(true);
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------

        private async Task RunWarmUp()
        {
            double step = 5;
            double threshold = 1;
            double maxDiff = 3;

            _warmUpCancellationToken = new CancellationTokenSource();

            _isWarmingUp = true;
            if (OnWarmUpStarted != null)
                OnWarmUpStarted();

            var targetTemp = double.IsNaN(AmbientTemperature) ? (DesiredCCDTemperature > 0 ? DesiredCCDTemperature : 0) : Camera.HeatSinkTemperature;
            do
            {
                var currentTemp = Camera.CCDTemperature;                

                var tempDiff = targetTemp - currentTemp;
                var sign = tempDiff / Math.Abs(tempDiff);
                var warmUpStep = Math.Abs(tempDiff) > step ? sign * step : tempDiff;

                Camera.SetCCDTemperature = Camera.CCDTemperature + warmUpStep;

                while (Math.Abs(Camera.SetCCDTemperature - Camera.CCDTemperature) > threshold)
                {
                    if (OnWarmUpProgressChanged != null)
                        OnWarmUpProgressChanged(targetTemp, currentTemp);

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), _warmUpCancellationToken.Token);
                    }
                    catch (Exception)
                    {
                        _logService.LogMessage(new LogMessage(this, LogEventCategory.Verbose, "Task.Delay exception thrown"));
                    }
                    
                    if (_warmUpCancellationToken.IsCancellationRequested)
                    {
                        _isWarmingUp = false;
                        if (OnWarmUpCanceled != null)
                            OnWarmUpCanceled();
                        _warmUpCancellationToken = null;
                        return;
                    }

                    if (Math.Abs(targetTemp - Camera.CCDTemperature) <= maxDiff)
                        break;
                }
            } while (Math.Abs(targetTemp - Camera.CCDTemperature) > maxDiff);

            _isWarmingUp = false;
            _logService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "CCD warmup sequence completed."));
            if (OnWarmUpCompleted != null)
                OnWarmUpCompleted();

        }

        /// <summary>
        /// Initialize periodic monitoring of the camera state.
        /// Monitoring the temperature to keep a temperature record history.
        /// </summary>
        private void InitializeMonitoring()
        {
            double currentTemp = Camera.Connected && Camera.CanSetCCDTemperature ? Camera.CCDTemperature : 0;
            for(int i = 0; i < StoredTemperatureHistoryLength; i++) 
                _cameraTemperatureHistory.Add(currentTemp);
            _temperatureMonitoringToken = new CancellationTokenSource();
            PeriodicWorkRunner.DoWorkAsync(new Action(GetCameraTemperature), TimeSpan.Zero,
                new TimeSpan(0, 0, TemperatureQueryInterval), false,
                _temperatureMonitoringToken.Token);
        }

        /// <summary>
        /// Gets the camera temperature and updates the temperature history buffer.
        /// </summary>
        private void GetCameraTemperature()
        {
            if (Camera.Connected)
            {
                if (Camera.CanSetCCDTemperature)
                {
                    var currentTemp = Camera.CCDTemperature;
                    while (_cameraTemperatureHistory.Count >= StoredTemperatureHistoryLength)
                    {
                        _cameraTemperatureHistory.RemoveAt(0);
                    }
                    _cameraTemperatureHistory.Add(currentTemp);
                }
            }
        }

        #endregion

    }
}
