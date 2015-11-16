using System;
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

        private readonly ICameraService _cameraService;
        private IViewProvider _viewProvider;
        private IView<ConnectDialogViewModel> _connectDialog;
        private IView<DeviceInfoViewModel> _deviceInfoDialog;

        public MainViewModel(ILogService logService, ICameraService cameraService, IViewProvider viewProvider)
            : base(logService)
        {
            _cameraService = cameraService;
            _viewProvider = viewProvider;
            ViewTitle = "DSImager";
        }

        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            _cameraService.OnCameraChosen += OnCameraChosen;
            _connectDialog = _viewProvider.Instantiate<ConnectDialogViewModel>();
            _connectDialog.ShowModal();
        }

        private void OnCameraChosen(string cameraName)
        {
            ViewTitle = "DSImager - " + cameraName;
        }

        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;
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
