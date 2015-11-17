using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DSImager.Core.Interfaces;

namespace DSImager.ViewModels
{
    public class MainViewModel : BaseViewModel<MainViewModel>
    {

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
        public List<double> PreviewExposureOptions
        {
            get { return _previewExposureOptions; }
            set
            {
                SetNotifyingProperty(() => PreviewExposureOptions, ref _previewExposureOptions, value);
            }
        }

        public ICamera ConnectedCamera { get { return _cameraService.ConnectedCamera; } }

        private readonly ICameraService _cameraService;
        private IApplication _application;
        private IViewProvider _viewProvider;
        private IView<ConnectDialogViewModel> _connectDialog;
        private IView<DeviceInfoViewModel> _deviceInfoDialog;

        private int _selectedPreviewExposureIndex = 0;
        public int SelectedPreviewExposureIndex 
        {
            get { return _selectedPreviewExposureIndex; }
            set { SetNotifyingProperty(() => SelectedPreviewExposureIndex, ref _selectedPreviewExposureIndex, value); }
        }

        public MainViewModel(ILogService logService, ICameraService cameraService, IViewProvider viewProvider,
            IApplication application)
            : base(logService)
        {
            _cameraService = cameraService;
            _application = application;
            _viewProvider = viewProvider;
            ViewTitle = "DSImager";
        }

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

        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;
        }

        private void PostInitialize()
        {
            // Construct ticks for the preview exposure chooser.
            // Limit maximum to 60 seconds.
            var minExposure = _cameraService.ConnectedCamera.Capabilities.MinExposure;
            var maxExposure = _cameraService.ConnectedCamera.Capabilities.MaxExposure;

            if (Double.IsInfinity(maxExposure) || Double.IsNaN(maxExposure) || maxExposure > 60)
                maxExposure = 60;

            List<double> ticks = new List<double>();
            ticks.Add(minExposure);

            var currentVal = minExposure;
            while (currentVal < 1)
            {
                currentVal *= 10;
                ticks.Add(currentVal);
            }

            ticks.AddRange(
                (new double[] {1, 2, 3, 5, 10, 15, 20, 30, 40, 50, 60}).Where(d => d <= maxExposure));
            PreviewExposureOptions = ticks;
        }

        private void OpenDeviceInfoDialog()
        {
            if(_deviceInfoDialog == null || _deviceInfoDialog.WasClosed)
                _deviceInfoDialog = _viewProvider.Instantiate<DeviceInfoViewModel>();
            _deviceInfoDialog.Show();
        }


        public ICommand OpenDeviceInfoDialogCommand { get { return new CommandHandler(OpenDeviceInfoDialog); } }
    }
}
// http://stackoverflow.com/questions/7877532/wpf-event-binding-from-view-to-viewmodel