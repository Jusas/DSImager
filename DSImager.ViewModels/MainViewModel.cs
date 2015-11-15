using System;
using DSImager.Core.Interfaces;

namespace DSImager.ViewModels
{
    public class MainViewModel : BaseViewModel<MainViewModel>
    {
        private readonly ICameraService _cameraService;
        private IViewProvider _viewProvider;
        private IView<ConnectDialogViewModel> _connectDialog;

        public MainViewModel(ILogService logService, ICameraService cameraService, IViewProvider viewProvider)
            : base(logService)
        {
            _cameraService = cameraService;
            _viewProvider = viewProvider;
        }

        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            _connectDialog = _viewProvider.Instantiate<ConnectDialogViewModel>();
            _connectDialog.ShowModal();
        }

        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;
        }
    }
}
