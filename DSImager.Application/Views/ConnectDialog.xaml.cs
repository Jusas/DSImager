using System;
using System.ComponentModel;
using System.Windows.Media.Animation;
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

            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // Just for some fanciness.
            if (propertyChangedEventArgs.PropertyName == "InitializationErrorMessage")
            {
                var sbShow = (Storyboard)FindResource("ShowError");
                var sbHide = (Storyboard)FindResource("HideError");
                Storyboard.SetTarget(sbShow.Children[0], this);
                Storyboard.SetTarget(sbHide.Children[0], this);

                if (string.IsNullOrEmpty(ViewModel.ViewModel.InitializationErrorMessage))
                {
                    sbHide.AutoReverse = false;
                    sbHide.Begin();
                }
                else
                {
                    sbShow.AutoReverse = false;
                    sbShow.Begin();
                }
            }
        }

    }
}
