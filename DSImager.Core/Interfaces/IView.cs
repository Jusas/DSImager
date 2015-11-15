using System;

namespace DSImager.Core.Interfaces
{
    public interface IView<TViewModel>
    {
        IViewModel<TViewModel> ViewModel { get; }

        event EventHandler OnViewLoaded;

        void Show();
        void Close();
        bool ShowModal();
    }
}
