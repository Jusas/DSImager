﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ASCOM.DeviceInterface;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;

namespace DSImager.ViewModels
{
    public class MainViewModel : BaseViewModel<MainViewModel>
    {

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
        public ICameraV2 ConnectedCamera { get { return _cameraService.ConnectedCamera; } }
        private readonly ICameraService _cameraService;


        private IApplication _application;
        private IViewProvider _viewProvider;

        /// <summary>
        /// Reference to a connect dialog instance (injected).
        /// </summary>
        private IView<ConnectDialogViewModel> _connectDialog;

        /// <summary>
        /// Reference to a device info dialog instance (injected).
        /// </summary>
        private IView<DeviceInfoViewModel> _deviceInfoDialog;

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

        private const int LogBufferSize = 30;
        private ObservableCollection<LogMessage> _logBuffer = new ObservableCollection<LogMessage>();
        /// <summary>
        /// Log messages that have been buffered for GUI.
        /// </summary>
        public ObservableCollection<LogMessage> LogMessages { get { return _logBuffer; } }

        public LogMessage LastLogMessage
        {
            get { return LogMessages.Count > 0 ? LogMessages[0] : new LogMessage(null, LogEventCategory.Informational, ""); }
        }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------


        public MainViewModel(ILogService logService, ICameraService cameraService, IViewProvider viewProvider,
            IApplication application)
            : base(logService)
        {
            _cameraService = cameraService;
            _application = application;
            _viewProvider = viewProvider;
            ViewTitle = "DSImager";

            LogMessages.CollectionChanged += (sender, args) => SetNotifyingProperty(() => LastLogMessage);
            LogMessages.Add(new LogMessage(null, LogEventCategory.Informational, ""));
        }


        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;
            // Listen to all events.
            LogService.Subscribe(LogService.GlobalLogSource,
                LogEventCategory.Error | LogEventCategory.Informational | LogEventCategory.Warning,
                OnLogMessage);
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------
        
        private void PostInitialize()
        {
            ConstructPreviewExposureOptions();
            ConstructBinningOptions();
        }

        private void ConstructPreviewExposureOptions()
        {
            // Construct ticks (or valid values) for the preview exposure chooser.
            // Limit maximum to 300 seconds.
            // The scale will be from supported minimum to supported maximum or 300s,
            // whichever is smaller.
            const int scaleMax = 300;

            var minExposure = _cameraService.ConnectedCamera.ExposureMin;
            var maxExposure = _cameraService.ConnectedCamera.ExposureMax;

            if (Double.IsInfinity(maxExposure) || Double.IsNaN(maxExposure) || maxExposure > scaleMax)
                maxExposure = scaleMax;

            List<double> ticks = new List<double>();
            ticks.Add(minExposure);

            var currentVal = minExposure;
            while (currentVal < 1)
            {
                currentVal *= 10;
                ticks.Add(currentVal);
            }

            ticks.AddRange(
                (new double[] { 1, 2, 3, 5, 10, 15, 20, 30, 40, 50, 60, 90, 120, 180, 300 }).Where(d => d <= maxExposure));
            PreviewExposureOptions = ticks;
            SelectedPreviewExposureIndex = 0;
        }

        private void ConstructBinningOptions()
        {
            var cam = _cameraService.ConnectedCamera;
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

        private void OpenDeviceInfoDialog()
        {
            if(_deviceInfoDialog == null || _deviceInfoDialog.WasClosed)
                _deviceInfoDialog = _viewProvider.Instantiate<DeviceInfoViewModel>();
            _deviceInfoDialog.Show();
        }

        private void SetBinning(object binningModeOption)
        {
            KeyValuePair<int, string> b = (KeyValuePair<int, string>) binningModeOption;
            var binning = BinningModeOptions.Where(o => o.Key == b.Key).FirstOrDefault();
            SelectedBinningModeIndex = BinningModeOptions.IndexOf(binning);
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

        private void OnCameraChosen(string cameraName)
        {
            ViewTitle = "DSImager - " + cameraName;
        }


        private void OnLogMessage(LogMessage logMessage)
        {
            while (_logBuffer.Count >= LogBufferSize)
            {
                _logBuffer.RemoveAt(_logBuffer.Count - 1);
            }
            _logBuffer.Insert(0, logMessage);
        }


        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand OpenDeviceInfoDialogCommand { get { return new CommandHandler(OpenDeviceInfoDialog); } }
        public ICommand SetBinningCommand { get { return new CommandHandler(SetBinning); } }


        #endregion
    }
}
// http://stackoverflow.com/questions/7877532/wpf-event-binding-from-view-to-viewmodel