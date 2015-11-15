using System;
using System.Windows;
using DSImager.Core.Interfaces;
using MahApps.Metro.Controls;

namespace DSImager.Application.Views
{
    public abstract class BaseView<TViewModel> : MetroWindow, IView<TViewModel>
    {

        public event EventHandler OnViewLoaded;
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
            Loaded += OnLoaded;
        }

        
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (OnViewLoaded != null)
                OnViewLoaded(sender, null);
        }
    }
}
