using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ASCOM.DeviceInterface;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using DSImager.Core.System;
using DSImager.ViewModels.States;

namespace DSImager.ViewModels
{
    public class MainViewModel : BaseViewModel<MainViewModel>
    {
        /// <summary>
        /// A little helper class to make the view bindings a little lighter.
        /// Populates fields that are shown in the pause pane.
        /// </summary>
        public class SessionInformation
        {
            public bool WasManuallyPaused { get; private set; }
            public bool InErrorState { get; private set; }
            public string PauseReason { get; private set; }
            
            public List<Tuple<string, string>> InfoItems { get; private set; }

            public SessionInformation(ImagingSession session, string pauseReason = "", bool wasManuallyPaused = false, bool inErrorState = false)
            {
                InfoItems = new List<Tuple<string, string>>();
                if (session != null)
                {
                    var sequences = session.ImageSequences;
                    var curSeqIndex = session.CurrentImageSequenceIndex;
                    var sequence = sequences[curSeqIndex];

                    if (wasManuallyPaused || inErrorState)
                    {
                        InfoItems.Add(new Tuple<string, string>(
                            "Currently processing sequence:", string.Format("{0} ({1}/{2})", sequence.Name, curSeqIndex + 1, sequences.Count)));
                        InfoItems.Add(new Tuple<string, string>(
                            "Currently processing exposure:", string.Format("{0}/{1}", sequence.CurrentExposureIndex+1, sequence.NumExposures)));
                    }
                    else
                    {
                        InfoItems.Add(new Tuple<string, string>(
                            "Completed sequence:", string.Format("{0} ({1} exposures)", sequences[curSeqIndex].Name, sequences[curSeqIndex].NumExposures)));
                        if (curSeqIndex != sequences.Count - 1)
                        {
                            InfoItems.Add(new Tuple<string, string>(
                                "Next sequence:", string.Format("{0} ({1} exposures)", sequences[curSeqIndex+1].Name, sequences[curSeqIndex+1].NumExposures)));
                        }

                        InfoItems.Add(new Tuple<string, string>(
                            "Session sequence progress:", string.Format("{0} of {1} done", curSeqIndex+1, session.ImageSequences.Count)));
                    }
                    
                    PauseReason = pauseReason;
                    InErrorState = inErrorState;
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private string _viewTitle;
        /// <summary>
        /// The title of the View.
        /// </summary>
        public string ViewTitle
        {
            get
            {
                return _viewTitle;
            }
            set
            {
                SetNotifyingProperty(() => ViewTitle, ref _viewTitle, value);
            }
        }

        private List<double> _previewExposureOptions;
        /// <summary>
        /// Preview exposure options, an array, in seconds.
        /// </summary>
        public List<double> PreviewExposureOptions
        {
            get { return _previewExposureOptions; }
            set
            {
                SetNotifyingProperty(() => PreviewExposureOptions, ref _previewExposureOptions, value);
            }
        }

        /// <summary>
        /// Reference to the currently connected camera.
        /// </summary>
        public ICameraV2 ConnectedCamera { get { return _cameraService.Camera; } }
        private readonly ICameraService _cameraService;


        private IApplication _application;
        private IViewProvider _viewProvider;
        private IImagingService _imagingService;

        /// <summary>
        /// Reference to a connect dialog instance
        /// </summary>
        private IView<ConnectDialogViewModel> _connectDialog;

        /// <summary>
        /// Reference to a device info dialog instance
        /// </summary>
        private IView<DeviceInfoViewModel> _deviceInfoDialog;

        /// <summary>
        /// Reference to the histogram dialog instance
        /// </summary>
        private IView<HistogramDialogViewModel> _histogramDialog;

        /// <summary>
        /// Reference to the session dialog instance
        /// </summary>
        private IView<SessionDialogViewModel> _sessionDialog;

        /// <summary>
        /// Reference to the temperature dialog instance
        /// </summary>
        private IView<TemperatureDialogViewModel> _temperatureDialog;

        private int _selectedPreviewExposureIndex = 0;
        /// <summary>
        /// The index of the selected preview exposure option - corresponds to
        /// PreviewExposureOptions.
        /// </summary>
        public int SelectedPreviewExposureIndex 
        {
            get { return _selectedPreviewExposureIndex; }
            set
            {
                SetNotifyingProperty(() => SelectedPreviewExposureIndex, ref _selectedPreviewExposureIndex, value);
                SetNotifyingProperty(() => SelectedPreviewExposure);
                // Cancel any ongoing exposure when the value is changed.
                _imagingService.CancelCurrentImagingOperation();
            }
        }

        /// <summary>
        /// Double value of the selected preview exposure value.
        /// </summary>
        public double SelectedPreviewExposure
        {
            get
            {
                return PreviewExposureOptions != null ? PreviewExposureOptions[_selectedPreviewExposureIndex] : 0;
            }
        }

        private bool _isPreviewRepeating = false;
        /// <summary>
        /// The preview exposure repeat setting.
        /// </summary>
        public bool IsPreviewRepeating
        {
            get
            {
                return _isPreviewRepeating;
            }
            set
            {
                SetNotifyingProperty(() => IsPreviewRepeating, ref _isPreviewRepeating, value);
            }
        }

        public int _selectedBinningModeIndex = 0;
        /// <summary>
        /// Binning mode index, selected from BinningModeOptions.
        /// </summary>
        public int SelectedBinningModeIndex
        {
            get
            {
                return _selectedBinningModeIndex;
            }
            set
            {
                SetNotifyingProperty(() => SelectedBinningModeIndex, ref _selectedBinningModeIndex, value);
                SetNotifyingProperty(() => SelectedBinningMode);
            }
        }

        /// <summary>
        /// The selected binning mode.
        /// </summary>
        public KeyValuePair<int, string> SelectedBinningMode
        {
            get { return _binningModeOptions != null ? _binningModeOptions[_selectedBinningModeIndex] : new KeyValuePair<int, string>(); }
        }

        private List<KeyValuePair<int, string>> _binningModeOptions;
        /// <summary>
        /// Different binning modes available for the connected camera.
        /// </summary>
        public List<KeyValuePair<int, string>> BinningModeOptions
        {
            get { return _binningModeOptions; }
            set
            {
                SetNotifyingProperty(() => BinningModeOptions, ref _binningModeOptions, value);
                SetNotifyingProperty(() => SelectedBinningMode);
            }
        }

        private bool _isExposuring = false;
        /// <summary>
        /// Is the camera currently exposuring.
        /// </summary>
        public bool IsExposuring
        {
            get { return _isExposuring; }
            set
            {
                SetNotifyingProperty(() => IsExposuring, ref _isExposuring, value);
            }
        }

        private bool _isPreviewingEnabled = true;
        /// <summary>
        /// Is previewing enabled.
        /// </summary>
        public bool IsPreviewingEnabled
        {
            get { return _isPreviewingEnabled; }
            set
            {
                SetNotifyingProperty(() => IsPreviewingEnabled, ref _isPreviewingEnabled, value);
            }
        }

        private int _currentExposureProgress = 0;
        /// <summary>
        /// Progress of the current exposure (shown in the progress bar)
        /// </summary>
        public int CurrentExposureProgress
        {
            get { return _currentExposureProgress; }
            set
            {
                SetNotifyingProperty(() => CurrentExposureProgress, ref _currentExposureProgress, value);
            }
        }

        private string _exposureStatusText = "";
        /// <summary>
        /// The text of the exposure progress bar showing the state of the exposuring.
        /// </summary>
        public string ExposureStatusText
        {
            get { return _exposureStatusText; }
            set
            {
                SetNotifyingProperty(() => ExposureStatusText, ref _exposureStatusText, value);
            }
        }

        private Exposure _lastExposure;
        /// <summary>
        /// The last exposure that was taken.
        /// </summary>
        public Exposure LastExposure
        {
            get { return _lastExposure; }
            private set
            {
                SetNotifyingProperty(() => LastExposure, ref _lastExposure, value);
            }
        }

        private MainViewState _uiState = MainViewState.Idle;

        public MainViewState UiState
        {
            get { return _uiState; }
            set
            {
                SetNotifyingProperty(() => UiState, ref _uiState, value);
            }
        }

        private bool _isInSession = false;
        /// <summary>
        /// Are we running an imaging session.
        /// </summary>
        public bool IsInSession
        {
            get { return _isInSession; }
            private set
            {
                SetNotifyingProperty(() => IsInSession, ref _isInSession, value);
            }
        }

        private bool _isSessionPaused = false;
        /// <summary>
        /// Is the current imaging session paused.
        /// </summary>
        public bool IsSessionPaused
        {
            get { return _isSessionPaused; }
            private set
            {
                SetNotifyingProperty(() => IsSessionPaused, ref _isSessionPaused, value);
            }
        }

        private ImagingSession _currentSession;
        /// <summary>
        /// Reference to the current imaging session.
        /// </summary>
        public ImagingSession CurrentSession
        {
            get { return _currentSession; }
            private set
            {
                SetNotifyingProperty(() => CurrentSession, ref _currentSession, value);
            }
        }

        private SessionInformation _currentSessionInformation = new SessionInformation(null);

        /// <summary>
        /// Session information in a convenient form.
        /// </summary>
        public SessionInformation CurrentSessionInformation
        {
            get
            {
                return _currentSessionInformation;
            }
            set
            {
                SetNotifyingProperty(() => CurrentSessionInformation, ref _currentSessionInformation, value);
            }
        }

        private const int LogBufferSize = 30;
        private ObservableCollection<LogMessage> _logBuffer = new ObservableCollection<LogMessage>();
        /// <summary>
        /// Log messages that have been buffered for GUI.
        /// </summary>
        public ObservableCollection<LogMessage> LogMessages { get { return _logBuffer; } }

        private LogMessage _lastLogMessage;
        public LogMessage LastLogMessage
        {
            private set
            {
                SetNotifyingProperty(() => LastLogMessage, ref _lastLogMessage, value);
            }
            get
            {
                return _lastLogMessage ?? new LogMessage(null, LogEventCategory.Informational, "Ready");
            }
        }

        private string _userPauseString = "Paused by user";

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public MainViewModel(ILogService logService, ICameraService cameraService, IImagingService imagingService, 
            IViewProvider viewProvider, IApplication application)
            : base(logService)
        {
            _cameraService = cameraService;
            _imagingService = imagingService;
            _application = application;
            _viewProvider = viewProvider;
            ViewTitle = "DSImager";

            LogMessages.CollectionChanged += (sender, args) =>
            {
                LastLogMessage = LogMessages[LogMessages.Count - 1];
            };
                
            LogMessages.Add(new LogMessage(null, LogEventCategory.Informational, "Starting up"));
        }


        public override void Initialize()
        {
            PropertyChanged += WatchPropertyChanges;
            OwnerView.OnViewLoaded += OnViewLoaded;
            OwnerView.OnViewClosed += OnViewClosed;
            // Listen to all events.
            LogService.Subscribe(LogService.GlobalLogSource,
                LogEventCategory.Error | LogEventCategory.Informational | LogEventCategory.Warning,
                OnLogMessage);
        }

        private void WatchPropertyChanges(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == GetPropertyName(() => IsInSession) ||
                e.PropertyName == GetPropertyName(() => IsSessionPaused) ||
                e.PropertyName == GetPropertyName(() => IsExposuring))
            {
                if (IsInSession && IsSessionPaused)
                    IsPreviewingEnabled = true;
                else if (!IsInSession)
                    IsPreviewingEnabled = true;
                else
                    IsPreviewingEnabled = false;
            }
        }


        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------
        
        private void PostInitialize()
        {
            ConstructPreviewExposureOptions();
            ConstructBinningOptions();
            SetupBindings();
        }

        private void ConstructPreviewExposureOptions()
        {
            // Construct ticks (or valid values) for the preview exposure chooser.
            // Limit maximum to 300 seconds.
            // The scale will be from supported minimum to supported maximum or 300s,
            // whichever is smaller.
            const int scaleMax = 300;

            var minExposure = _cameraService.Camera.ExposureMin;
            var maxExposure = _cameraService.Camera.ExposureMax;

            if (Double.IsInfinity(maxExposure) || Double.IsNaN(maxExposure) || maxExposure > scaleMax)
                maxExposure = scaleMax;

            List<double> ticks = new List<double>();

            var currentVal = minExposure;
            while (currentVal < 1)
            {
                ticks.Add(currentVal);
                currentVal *= 10;                
            }

            ticks.AddRange(
                (new double[] { 1, 2, 3, 5, 10, 15, 20, 30, 40, 50, 60, 90, 120, 180, 300 }).Where(d => d <= maxExposure));
            PreviewExposureOptions = ticks;
            SelectedPreviewExposureIndex = 0;
        }

        private void ConstructBinningOptions()
        {
            var cam = _cameraService.Camera;
            // For now assume we have equal X and Y binning. Otherwise assume no support.
            var maxBinning = cam.MaxBinX == cam.MaxBinY ? cam.MaxBinX : 1;

            List<KeyValuePair<int, string>> opts = new List<KeyValuePair<int, string>>();
            for (int i = 0; i < maxBinning; i++)
            {
                opts.Add(new KeyValuePair<int, string>(i+1, string.Format("{0}x{0}",i+1)));
            }

            BinningModeOptions = opts;
            SelectedBinningModeIndex = 0;
        }

        private void SetupBindings()
        {
            _cameraService.OnExposureStarted += OnExposureStarted;
            _cameraService.OnExposureProgressChanged += OnExposureProgressChanged;
            _cameraService.OnExposureCompleted += OnExposureCompleted;
            _imagingService.OnImagingComplete += OnImagingComplete;
            
            _imagingService.OnImagingSessionPaused += OnImagingSessionPaused;
            _imagingService.OnImagingSessionResumed += OnImagingSessionResumed;
            _imagingService.OnImagingSessionCompleted += OnImagingSessionCompleted;
            _imagingService.OnImagingSessionStarted += OnImagingSessionStarted;
            _imagingService.OnImageSequenceStarted += OnImageSequenceStarted;
            _imagingService.OnImageSequenceCompleted += OnImageSequenceCompleted;
        }

        private void OnImageSequenceCompleted(ImagingSession session, ImageSequence sequence)
        {
            LogService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Image sequence '" + sequence.Name + "' completed."));
        }

        private void OnImageSequenceStarted(ImagingSession session, ImageSequence sequence)
        {
            string logMsg = "";
            if (sequence.CurrentExposureIndex == 0)
                logMsg += "Image sequence '" + sequence.Name + "' started.";
            else
            {
                logMsg += "Resuming image sequence '" + sequence.Name + "'.";
            }
            LogService.LogMessage(new LogMessage(this, LogEventCategory.Informational, logMsg));
        }

        private void OnExposureStarted(double duration)
        {
            IsExposuring = true;
            LogService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Exposure started (" + duration + "s)"));
        }

        private void OnImagingSessionStarted(ImagingSession session)
        {
            IsInSession = true;
            CurrentSession = session;
            LogService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Started imaging session '" + session.Name + "'."));
        }

        private void OnImagingSessionCompleted(ImagingSession session, bool completedSuccessfully, bool canceledByUser)
        {
            IsSessionPaused = false;
            IsInSession = false;
            CurrentSessionInformation = null;
            CurrentSession = null;
            LogService.LogMessage(new LogMessage(this, LogEventCategory.Informational,
                "Imaging session '" + session.Name + "' completed " + 
                (completedSuccessfully ? "successfully" : "unsuccessfully") + 
                (canceledByUser ? ", the session was canceled by user." : "") ));
        }

        private void OnImagingSessionResumed(ImagingSession session)
        {
            IsSessionPaused = false;
            LogService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Resuming session '" + session.Name + "'."));
        }

        private void OnImagingSessionPaused(ImagingSession session, string reason, bool error)
        {
            IsSessionPaused = true;
            CurrentSessionInformation = new SessionInformation(session, reason, reason == _userPauseString, error);
            LogService.LogMessage(new LogMessage(this, LogEventCategory.Informational, "Imaging session '" + session.Name + "' has been paused."));
        }


        private void OpenDeviceInfoDialog()
        {
            if(_deviceInfoDialog == null || _deviceInfoDialog.WasClosed)
                _deviceInfoDialog = _viewProvider.Instantiate<DeviceInfoViewModel>();
            _deviceInfoDialog.Show();
        }

        private void OpenHistogramDialog()
        {
            if (_histogramDialog == null || _histogramDialog.WasClosed)
                _histogramDialog = _viewProvider.Instantiate<HistogramDialogViewModel>();
            _histogramDialog.Show();
        }

        private void OpenLogFile()
        {
            System.Diagnostics.Process.Start(LogService.LogFile);
        }

        private void OpenSessionDialog()
        {
            if (_sessionDialog == null || _sessionDialog.WasClosed)
                _sessionDialog = _viewProvider.Instantiate<SessionDialogViewModel>();
            _sessionDialog.Show();
        }

        private void OpenTemperatureDialog()
        {
            if (_temperatureDialog == null || _temperatureDialog.WasClosed)
                _temperatureDialog = _viewProvider.Instantiate<TemperatureDialogViewModel>();
            _temperatureDialog.Show();
        }

        private void SetBinning(object binningModeOption)
        {
            KeyValuePair<int, string> b = (KeyValuePair<int, string>) binningModeOption;
            var binning = BinningModeOptions.Where(o => o.Key == b.Key).FirstOrDefault();
            SelectedBinningModeIndex = BinningModeOptions.IndexOf(binning);
        }

        private void PreviewExposure()
        {
            // If already previewing, we mean to stop/abort capturing.
            if (UiState == MainViewState.Imaging)
            {
                IsPreviewRepeating = false;
                _imagingService.CancelCurrentImagingOperation();                
            }
            else if(UiState == MainViewState.Idle)
            {
                IsExposuring = true;
                UiState = MainViewState.Imaging; // todo: map out the normal cycle of states. Do this before implementing stop/abort etc.
                _imagingService.TakeSingleExposure(SelectedPreviewExposure, SelectedBinningMode.Key,
                    null);    
            }
            
        }

        /// <summary>
        /// Pauses the capturing operation.
        /// </summary>
        private void PauseCapture()
        {
            _imagingService.PauseCurrentImagingOperation(_userPauseString, false);
        }

        /// <summary>
        /// Stops the session capture operation.
        /// </summary>
        private void StopCapture()
        {
            _imagingService.CancelStoredImagingOperation();
        }

        /// <summary>
        /// Resume the capture session.
        /// </summary>
        private void ResumeCapture()
        {
            _imagingService.ResumeStoredImagingOperation();
        }

        // Event handlers

        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            _cameraService.OnCameraChosen += OnCameraChosen;
            _connectDialog = _viewProvider.Instantiate<ConnectDialogViewModel>();
            bool quit = !_connectDialog.ShowModal();
            if (quit)
                _application.ExitApplication(0);

            PostInitialize();
        }


        private void OnViewClosed(object sender, EventArgs e)
        {
            _application.ExitApplication(0);
        }

        private void OnCameraChosen(string cameraName)
        {
            ViewTitle = "DSImager - " + cameraName;
        }


        private void OnLogMessage(LogMessage logMessage)
        {
            while (LogMessages.Count >= LogBufferSize)
            {
                LogMessages.RemoveAt(0);
            }
            LogMessages.Add(logMessage);
        }

        private void OnExposureProgressChanged(double currentExposureDuration, double targetExposureDuration, ExposurePhase phase)
        {
            // Update the progress bar text and value.
            CurrentExposureProgress = (int)Math.Min(100.0, (currentExposureDuration / targetExposureDuration * 100.0));
            var exposureNum = _imagingService.CurrentImageSequence != null ? _imagingService.CurrentImageSequence.CurrentExposureIndex + 1 : 1;
            var totalExposures = _imagingService.CurrentImageSequence != null ? _imagingService.CurrentImageSequence.NumExposures : 1;
            double remainingExposureTime = _imagingService.CurrentImageSequence != null ? (totalExposures - exposureNum + 1) *
                                           _imagingService.CurrentImageSequence.ExposureDuration - currentExposureDuration
                                           : targetExposureDuration - currentExposureDuration;
            var remainingStr = TimeSpan.FromSeconds(remainingExposureTime).ToString("hh\\:mm\\:ss");
            if (phase == ExposurePhase.Exposuring)
            {
                ExposureStatusText = string.Format("Sequence exposure {0}/{1} | Progress: {2}% | Total Time Remaining: {3}",
                    exposureNum, totalExposures, CurrentExposureProgress, remainingStr);    
            }
            else if (phase == ExposurePhase.Downloading)
            {
                ExposureStatusText = string.Format("Sequence exposure {0}/{1} | Downloading... | Total Time Remaining: {2}",
                    exposureNum, totalExposures, remainingStr);    
            }
        }

        private void OnExposureCompleted(bool successful, Exposure exposure)
        {
            // TODO: Set IsExposuring to false but only if it was the last image of the session?
            IsExposuring = false;
            CurrentExposureProgress = 0;
            ExposureStatusText = "";
        }


        private void OnImagingComplete(bool successful, Exposure exposure)
        {
            if(LastExposure != null)
                LastExposure.OnHistogramStretchChanged -= OnExposureHistogramStretchChanged;

            LastExposure = exposure;
            LastExposure.OnHistogramStretchChanged += OnExposureHistogramStretchChanged;

            if (UiState == MainViewState.Imaging && IsPreviewRepeating)
            {
                UiState = MainViewState.Idle;
                PreviewExposure();
            }
            else
            {
                UiState = MainViewState.Idle;
            }
        }

        private void OnExposureHistogramStretchChanged()
        {
            // This will notify that the exposure has changed (although it really hasn't,
            // only its 8-bit pixel values have) to trigger the converter to update the
            // visual image.
            SetNotifyingProperty(() => LastExposure);
        }


        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand OpenDeviceInfoDialogCommand { get { return new CommandHandler(OpenDeviceInfoDialog); } }
        public ICommand OpenHistogramDialogCommand { get { return new CommandHandler(OpenHistogramDialog); } }
        public ICommand SetBinningCommand { get { return new CommandHandler(SetBinning); } }
        public ICommand PreviewExposureCommand { get { return new CommandHandler(PreviewExposure); } }
        public ICommand PauseCaptureCommand { get { return new CommandHandler(PauseCapture); } }
        public ICommand StopCaptureCommand { get { return new CommandHandler(StopCapture); } }
        public ICommand ResumeCaptureCommand { get { return new CommandHandler(ResumeCapture); } }
        public ICommand OpenLogFileCommand { get { return new CommandHandler(OpenLogFile); } }
        public ICommand OpenSessionDialogCommand { get { return new CommandHandler(OpenSessionDialog); } }
        public ICommand OpenTemperatureDialogCommand { get { return new CommandHandler(OpenTemperatureDialog); } }

        #endregion
    }

    /// <summary>
    /// Mockup, for design time use only - to satisfy the need of parameterless constructor for the DataContext
    /// and to enable design time visual feedback.
    /// </summary>
    public class MainViewModelDT : MainViewModel
    {
        public MainViewModelDT() : base(null, null, null, null, null)
        {
            
        }
        public MainViewModelDT(ILogService logService, ICameraService cameraService, IImagingService imagingService, IViewProvider viewProvider, IApplication application) : base(logService, cameraService, imagingService, viewProvider, application)
        {
        }
    }
}
// http://stackoverflow.com/questions/7877532/wpf-event-binding-from-view-to-viewmodel
