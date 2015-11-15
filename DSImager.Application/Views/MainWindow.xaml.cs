using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DSImager.ViewModels;

namespace DSImager.Application.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        
        public MainWindow(MainViewModel viewModel)
        {            
            InitializeComponent();
            ViewModel = viewModel;
        }
        

        private void OnPreviewExposureClicked(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            b.ContextMenu.IsEnabled = true;
            b.ContextMenu.PlacementTarget = b;
            b.ContextMenu.Placement = PlacementMode.Bottom;
            b.ContextMenu.IsOpen = true;

            /* https://joshsmithonwpf.wordpress.com/2007/09/14/modifying-the-auto-tooltip-of-a-slider/ */

        }

        private void OnPreviewBinningClicked(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            b.ContextMenu.IsEnabled = true;
            b.ContextMenu.PlacementTarget = b;
            b.ContextMenu.Placement = PlacementMode.Bottom;
            b.ContextMenu.IsOpen = true;
        }
    }
}
