using System;
using System.ComponentModel;
using System.Windows;
using DSImager.Core.Interfaces;
using MahApps.Metro.Controls;

namespace DSImager.Application.Views
{
    public abstract class BaseView<TViewModel> : MetroWindow, IView<TViewModel>
    {

        public event EventHandler OnViewLoaded;
        public event EventHandler<CancelEventArgs> OnViewClosing;
        public event EventHandler OnViewClosed;

        public bool WasClosed { get { return !IsLoaded; } }

        public bool ShowModal()
        {            
            var result = ShowDialog();
            return result ?? false;
        }

        private IViewModel<TViewModel> _viewModel;
        public IViewModel<TViewModel> ViewModel
        {
            protected set
            {
                _viewModel = value;
                _viewModel.OwnerView = this;
                _viewModel.Initialize();
                DataContext = value.ViewModel;                
            }
            get { return _viewModel; }
        }

        protected BaseView()
        {
            InitializeInterfaceBindings();
        }

        protected void InitializeInterfaceBindings()
        {
            Loaded += OnLoadedHandler;
            Closing += OnClosingHandler;
            Closed += OnViewClosedHandler;
        }

        private void OnClosingHandler(object sender, CancelEventArgs cancelEventArgs)
        {
            if (OnViewClosing != null)
                OnViewClosing(sender, cancelEventArgs);
        }

        private void OnViewClosedHandler(object sender, EventArgs args)
        {
            if (OnViewClosed != null)
                OnViewClosed(sender, args);
        }

        private void OnLoadedHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            if (OnViewLoaded != null)
                OnViewLoaded(sender, null);
        }
    }
}
