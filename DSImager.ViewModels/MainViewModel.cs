using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DSImager.Core.Interfaces;

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
        public ICamera ConnectedCamera { get { return _cameraService.ConnectedCamera; } }
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
                SetNotifyingProperty(() => SelectedPreviewExposureStringValue);
            }
        }

        private string _selectedPreviewExposureStringValue = "0";
        /// <summary>
        /// A string value of the selected preview exposure value.
        /// </summary>
        public string SelectedPreviewExposureStringValue
        {
            get
            {
                return PreviewExposureOptions != null ? PreviewExposureOptions[_selectedPreviewExposureIndex] + "s" : "";
            }
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
        }


        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------


        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            _cameraService.OnCameraChosen += OnCameraChosen;
            _connectDialog = _viewProvider.Instantiate<ConnectDialogViewModel>();
            bool quit = !_connectDialog.ShowModal();
            if(quit)
                _application.ExitApplication(0);

            PostInitialize();
        }

        private void OnCameraChosen(string cameraName)
        {
            ViewTitle = "DSImager - " + cameraName;
        }

        private void PostInitialize()
        {
            // Construct ticks (or valid values) for the preview exposure chooser.
            // Limit maximum to 300 seconds.
            // The scale will be from supported minimum to supported maximum or 300s,
            // whichever is smaller.
            const int scaleMax = 300;
            
            var minExposure = _cameraService.ConnectedCamera.Capabilities.MinExposure;
            var maxExposure = _cameraService.ConnectedCamera.Capabilities.MaxExposure;

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
                (new double[] {1, 2, 3, 5, 10, 15, 20, 30, 40, 50, 60, 90, 120, 180, 300}).Where(d => d <= maxExposure));
            PreviewExposureOptions = ticks;
            SelectedPreviewExposureIndex = 0;
        }

        private void OpenDeviceInfoDialog()
        {
            if(_deviceInfoDialog == null || _deviceInfoDialog.WasClosed)
                _deviceInfoDialog = _viewProvider.Instantiate<DeviceInfoViewModel>();
            _deviceInfoDialog.Show();
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand OpenDeviceInfoDialogCommand { get { return new CommandHandler(OpenDeviceInfoDialog); } }

        #endregion
    }
}
// http://stackoverflow.com/questions/7877532/wpf-event-binding-from-view-to-viewmodel