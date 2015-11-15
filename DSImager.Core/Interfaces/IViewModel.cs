namespace DSImager.Core.Interfaces
{
    public interface IViewModel<TViewModel>
    {
        TViewModel ViewModel { get; }
        IView<TViewModel> OwnerView { get; set; }
        void Initialize();
    }
}
