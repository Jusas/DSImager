using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using Newtonsoft.Json.Linq;

namespace DSImager.ViewModels
{
    public class FlatFrameDialogViewModel : BaseViewModel<FlatFrameDialogViewModel>
    {
        public class SampleFrameInfo
        {
            public double ExposureTime { get; set; }
            public double AduPcnt { get; set; }
        }

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private ICameraService _cameraService;
        private IImagingService _imagingService;
        private IStorageService _storageService;
        private IImageIoService _imageIoService;
        private IDialogProvider _dialogProvider;
        private ISystemEnvironment _systemEnvironment;
        private IProgramSettingsManager _programSettingsManager;
        private IApplication _application;
        private ITargetAduFinder _targetAduFinder;


        private List<string> _fileTypeOptionIds;
        public List<string> FileTypeOptionIds
        {
            get { return _fileTypeOptionIds; }
            set
            {
                SetNotifyingProperty(() => FileTypeOptionIds, ref _fileTypeOptionIds, value);
            }
        }

        private Dictionary<string, string> _fileTypeOptionNames;
        public Dictionary<string, string> FileTypeOptionNames
        {
            get { return _fileTypeOptionNames; }
            set
            {
                SetNotifyingProperty(() => FileTypeOptionNames, ref _fileTypeOptionNames, value);
            }
        }
        

        private ProgramSettings.FlatFrameDialogSettings _settings;
        public ProgramSettings.FlatFrameDialogSettings Settings
        {
            get { return _settings; }
            set
            {
                SetNotifyingProperty(() => Settings, ref _settings, value);
            }
        }
        
        private List<int> _binningModeOptions;
        /// <summary>
        /// Different binning modes available for the connected camera.
        /// </summary>
        public List<int> BinningModeOptions
        {
            get { return _binningModeOptions; }
            set
            {
                SetNotifyingProperty(() => BinningModeOptions, ref _binningModeOptions, value);
            }
        }
        
        private int _maxADU;
        public int MaxADU
        {
            get { return _maxADU; }
            set
            {
                SetNotifyingProperty(() => MaxADU, ref _maxADU, value);
            }
        }

        private bool _isAutoDetecting;
        public bool IsAutoDetecting
        {
            get { return _isAutoDetecting; }
            set
            {
                SetNotifyingProperty(() => IsAutoDetecting, ref _isAutoDetecting, value);
            }
        }

        private bool _hadFindException;
        public bool HadFindException
        {
            get { return _hadFindException; }
            set
            {
                SetNotifyingProperty(() => HadFindException, ref _hadFindException, value);
            }
        }

        private string _findExceptionMessage;
        public string FindExceptionMessage
        {
            get { return _findExceptionMessage; }
            set
            {
                SetNotifyingProperty(() => FindExceptionMessage, ref _findExceptionMessage, value);
            }
        }

        private SampleFrameInfo _lastSampleFrameInfo;
        public SampleFrameInfo LastSampleFrameInfo
        {
            get { return _lastSampleFrameInfo; }
            set
            {
                SetNotifyingProperty(() => LastSampleFrameInfo, ref _lastSampleFrameInfo, value);
            }
        }

        private bool _autoDetectCompleted;
        public bool AutoDetectCompleted
        {
            get { return _autoDetectCompleted; }
            set
            {
                SetNotifyingProperty(() => AutoDetectCompleted, ref _autoDetectCompleted, value);
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public FlatFrameDialogViewModel(ILogService logService,ICameraService cameraService,
            IImagingService imagingService, IStorageService storageService, IImageIoService imageIoService,
            IDialogProvider dialogProvider, ISystemEnvironment systemEnvironment, 
            IProgramSettingsManager programSettingsManager,
            IApplication application, ITargetAduFinder targetAduFinder) : base(logService)
        {
            _cameraService = cameraService;
            _imagingService = imagingService;
            _storageService = storageService;
            _imageIoService = imageIoService;
            _dialogProvider = dialogProvider;
            _systemEnvironment = systemEnvironment;
            _programSettingsManager = programSettingsManager;
            _application = application;
            _targetAduFinder = targetAduFinder;
        }

        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;
            OwnerView.OnViewClosing += OnViewClosing;
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------
        
        private void OnViewClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            SaveSettings();
        }

        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            LoadSettings();
            ConstructFileTypeOptions();
            ConstructBinningOptions();
            MaxADU = _cameraService.Camera.MaxADU;
        }

        private void LoadSettings()
        {
            Settings = _programSettingsManager.Settings.FlatFrameDialog;
        }

        private void SaveSettings()
        {
            _programSettingsManager.SaveSettings();
        }

        private void ConstructBinningOptions()
        {
            var cam = _cameraService.Camera;
            // For now assume we have equal X and Y binning. Otherwise assume no support.
            var maxBinning = cam.MaxBinX == cam.MaxBinY ? cam.MaxBinX : 1;

            List<int> opts = new List<int>();
            for (int i = 0; i < maxBinning; i++)
            {
                opts.Add(i + 1);
            }

            BinningModeOptions = opts;
        }

        private void ConstructFileTypeOptions()
        {
            FileTypeOptionIds = new List<string>(_imageIoService.WritableFileFormats.Select(wff => wff.Id));
            FileTypeOptionNames = new Dictionary<string, string>();
            foreach (var ff in _imageIoService.WritableFileFormats)
                FileTypeOptionNames.Add(ff.Id, ff.Name);
        }

        private void StartCapture()
        {
            ImagingSession session = new ImagingSession()
            {
                SaveOutput = true,
                OutputDirectory = Settings.SavePath,
                Name = ImagingSession.Calibration
            };
            
            ImageSequence sequence = new ImageSequence()
            {
                Name = "Calibration",
                BinXY = Settings.BinningModeXY,
                Extension = "_Flat",
                ExposureDuration = Settings.ExposureTime, // Zero should be acceptable for bias frames (== minimum exposure value) in ASCOM standard
                FileFormat = Settings.FileFormat,
                NumExposures = Settings.FrameCount,
            };
            session.ImageSequences.Add(sequence);

            _imagingService.OnImagingSessionCompleted += ImagingSessionCompletedHandler;

            // Waiting for the window to disappear before starting (a bit of a hax).            
            Task.Delay(300).ContinueWith((t) => _imagingService.RunImagingSession(session));

            if(_application.IsInLightOverlayMode)
                _application.SetApplicationSessionVariable("rendering", false);

            //_imagingService.RunImagingSession(session);

            OwnerView.Close();
        }

        private void ImagingSessionCompletedHandler(ImagingSession session, bool completedSuccessfully, bool canceledByUser)
        {
            _imagingService.OnImagingSessionCompleted -= ImagingSessionCompletedHandler;
            if (_application.IsInLightOverlayMode)
            {
                _application.SetApplicationSessionVariable("rendering", true);
                ToggleWhiteMode();
            }
        }

        private void CancelCapture()
        {
            if(_application.IsInLightOverlayMode)
                ToggleWhiteMode();

            StopExposureAutoDetection();
            
            OwnerView.Close();
        }

        /// <summary>
        /// Select output directory from a directory dialog.
        /// </summary>
        private void SelectOutputDirectory()
        {
            string directory = string.IsNullOrEmpty(Settings.SavePath) ? _systemEnvironment.UserPicturesDirectory : Settings.SavePath;
            bool ok = _dialogProvider.ShowPickDirectoryDialog("Select output directory", directory,
                out directory);

            if (ok && !string.IsNullOrEmpty(directory))
            {
                Settings.SavePath = directory;
                SetNotifyingProperty(() => Settings);
            }
        }

        private void ToggleWhiteMode()
        {
            _application.SetLightOverlayMode(!_application.IsInLightOverlayMode);
        }

        private async void RunExposureAutoDetection()
        {
            try
            {
                LastSampleFrameInfo = null;
                AutoDetectCompleted = false;
                HadFindException = false;
                IsAutoDetecting = true;
                _application.SetApplicationSessionVariable("rendering", false);
                _targetAduFinder.OnFindExposureTaken += TargetAduFinderExposureTakenHandler;
                Settings.ExposureTime = await _targetAduFinder.FindExposureValue(Settings.TargetADU, Settings.MaxExposureToTry);
                _application.SetApplicationSessionVariable("rendering", true);
                SetNotifyingProperty(() => Settings);
                IsAutoDetecting = false;
                AutoDetectCompleted = true;
            }
            catch (Exception e)
            {
                IsAutoDetecting = false;
                HadFindException = true;
                FindExceptionMessage = e.Message;                
                LogService.LogMessage(new LogMessage(this, LogEventCategory.Error, "Exception raised during exposure auto-detection: " + e.Message));
                _application.SetApplicationSessionVariable("rendering", true);
            }
            
        }

        private void TargetAduFinderExposureTakenHandler(double usedExposureTime, int resultingAdu)
        {
            LastSampleFrameInfo = new SampleFrameInfo()
            {
                ExposureTime = usedExposureTime,
                AduPcnt = (double)resultingAdu / _cameraService.Camera.MaxADU * 100.0
            };
        }

        private void StopExposureAutoDetection()
        {
            _targetAduFinder.CancelFindOperation();
            IsAutoDetecting = false;
            _application.SetApplicationSessionVariable("rendering", true);
        }


        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand StartCaptureCommand { get { return new CommandHandler(StartCapture); } }
        public ICommand CancelCaptureCommand { get { return new CommandHandler(CancelCapture); } }
        public ICommand SelectOutputDirectoryCommand { get { return new CommandHandler(SelectOutputDirectory); } }
        public ICommand ToggleWhiteModeCommand { get { return new CommandHandler(ToggleWhiteMode);} }
        public ICommand RunExposureAutoDetectionCommand { get { return new CommandHandler(RunExposureAutoDetection); } }
        public ICommand StopExposureAutoDetectionCommand { get { return new CommandHandler(StopExposureAutoDetection); } }

        #endregion
    }


    // ReSharper disable once InconsistentNaming
    public class FlatFrameDialogViewModelDT : FlatFrameDialogViewModel
    {
        public FlatFrameDialogViewModelDT() : base(null, null, null, null, null, null, null, null, null, null)
        {
            LastSampleFrameInfo = new SampleFrameInfo() {AduPcnt = 15.5, ExposureTime = 0.55};
            HadFindException = true;
            FindExceptionMessage =
                "The exposure time required is estimated to be 546456, which is larger than given maximum exposure value";
            MaxADU = 35000;
            Settings = new ProgramSettings.FlatFrameDialogSettings()
            {
                BinningModeXY = 1,
                ExposureTime = 5,
                FileFormat = "fits",
                FrameCount = 1,
                MaxExposureToTry = 3,
                SavePath = @"c:\",
                TargetADU = 20000
            };
        }
    }
}
