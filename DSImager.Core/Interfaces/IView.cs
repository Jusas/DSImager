using System;
using System.ComponentModel;

namespace DSImager.Core.Interfaces
{
    public interface IView<TViewModel>
    {
        IViewModel<TViewModel> ViewModel { get; }
        bool WasClosed { get; }
        bool? DialogResult { get; set; }

        event EventHandler OnViewLoaded;
        event EventHandler<CancelEventArgs> OnViewClosing;
        event EventHandler OnViewClosed;

        void Show();
        void Close();
        bool ShowModal();
    }
}
