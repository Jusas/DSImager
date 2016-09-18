using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
            // Setup Quick Converter.
            QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
            QuickConverter.EquationTokenizer.AddNamespace("System", Assembly.GetAssembly(typeof(object)));
            QuickConverter.EquationTokenizer.AddNamespace(typeof(System.Windows.Visibility));
            QuickConverter.EquationTokenizer.AddAssembly(Assembly.GetAssembly(typeof(System.Windows.Visibility)));

            InitializeComponent();
            ViewModel = viewModel;

            // TODO: Implement these in a proper way. Hotkeys are great.
            RoutedCommand rc = new RoutedCommand();
            rc.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.None));            
            CommandBindings.Add(new CommandBinding(rc, (sender, args) => viewModel.PreviewExposureCommand.Execute(null)));

            rc = new RoutedCommand();
            rc.InputGestures.Add(new KeyGesture(Key.D1, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rc, (sender, args) => viewModel.SelectedPreviewExposureIndex = 3));

            rc = new RoutedCommand();
            rc.InputGestures.Add(new KeyGesture(Key.D2, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rc, (sender, args) => viewModel.SelectedPreviewExposureIndex = 4));

            rc = new RoutedCommand();
            rc.InputGestures.Add(new KeyGesture(Key.D3, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rc, (sender, args) => viewModel.SelectedPreviewExposureIndex = 5));

            rc = new RoutedCommand();
            rc.InputGestures.Add(new KeyGesture(Key.D4, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rc, (sender, args) => viewModel.SelectedPreviewExposureIndex = 6));

            rc = new RoutedCommand();
            rc.InputGestures.Add(new KeyGesture(Key.D5, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rc, (sender, args) => viewModel.SelectedPreviewExposureIndex = 7));
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

        private void OnLatestLogButtonClicked(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            b.ContextMenu.IsEnabled = true;
            b.ContextMenu.PlacementTarget = b;
            b.ContextMenu.Placement = PlacementMode.Top;
            b.ContextMenu.IsOpen = true;
        }

        private void OnLogListMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
