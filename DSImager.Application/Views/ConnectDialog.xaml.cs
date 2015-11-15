using DSImager.ViewModels;

namespace DSImager.Application.Views
{
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog
    {

        public ConnectDialog(ConnectDialogViewModel viewModel)
        {            
            InitializeComponent();
            ViewModel = viewModel;
        }
        
    }
}
