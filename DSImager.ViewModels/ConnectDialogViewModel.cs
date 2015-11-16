using System.Windows.Input;
using DSImager.Core.Interfaces;

namespace DSImager.ViewModels
{
    /// <summary>
    /// ViewModel for the ConnectDialog.
    /// </summary>
    public class ConnectDialogViewModel : BaseViewModel<ConnectDialogViewModel>
    {
        //-------------------------------------------------------------------------------------------------------
        #region INTERNAL CLASSES AND DECLARATIONS
        //-------------------------------------------------------------------------------------------------------

        /// <summary>
        /// State of the dialog.
        /// </summary>
        public enum DialogState
        {
            CameraNotChosen,
            CameraChosen,
            CameraConnecting,
            CameraConnected
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private ICameraService _cameraService;

        private readonly string _nothingSelected = "Not selected";
        
        private string _selectedDeviceId;
        public string SelectedDeviceId
        {
            get { return _selectedDeviceId; }
            set { SetNotifyingProperty(() => SelectedDeviceId, ref _selectedDeviceId, value); }
        }
        
        private DialogState _state;
        public DialogState State
        {
            get { return _state; }
            set
            {
                SetNotifyingProperty(() => State, ref _state, value);
                SetNotifyingProperty(() => CanConnectToCamera);
                SetNotifyingProperty(() => IsUiResponsive);
                SetNotifyingProperty(() => CameraConnectError);
            }
        }

        public string CameraConnectError
        {
            get { return _cameraService.LastError; }
        }

        private string _initializationErrorMessage = "";
        public string InitializationErrorMessage
        {
            get { return _initializationErrorMessage; }
            set { SetNotifyingProperty(() => InitializationErrorMessage, ref _initializationErrorMessage, value); }
        }

        public bool CanConnectToCamera
        {            
            get { return State == DialogState.CameraChosen; }
        }

        public bool IsUiResponsive
        {
            get { return State != DialogState.CameraConnecting; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------


        public ConnectDialogViewModel(ILogService logService, ICameraService cameraService)
            : base(logService)
        {
            _cameraService = cameraService;
            SelectedDeviceId = _nothingSelected;
            State = DialogState.CameraNotChosen;
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------

        private void OpenChooser()
        {
            var deviceId = _cameraService.ChooseDevice();
            SelectedDeviceId = string.IsNullOrEmpty(deviceId) ? _nothingSelected : deviceId;
            if (_selectedDeviceId != _nothingSelected)
            {
                State = DialogState.CameraChosen;
            }
            else
            {
                State = DialogState.CameraNotChosen;
            }
            InitializationErrorMessage = "";
        }

        private void ConnectCamera()
        {
            InitializationErrorMessage = "";
            State = DialogState.CameraConnecting;
            var connected = _cameraService.Initialize(_selectedDeviceId);
            State = connected ? DialogState.CameraConnected : DialogState.CameraChosen;

            if (!connected || !_cameraService.Initialized)
            {
                InitializationErrorMessage = _cameraService.LastError;
            }
            else
            {
                // We're connected! Close the dialog.
                OwnerView.Close();
            }

        }

        private void Quit()
        {
            OwnerView.Close();
            // todo: refer to IApplication or something
            //Application.Current.Shutdown();
        }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand ChooseCommand { get { return new CommandHandler(OpenChooser); } }
        public ICommand ConnectCameraCommand { get { return new CommandHandler(ConnectCamera); } }
        public ICommand QuitCommand { get { return new CommandHandler(Quit); } }

        #endregion

    }
}
