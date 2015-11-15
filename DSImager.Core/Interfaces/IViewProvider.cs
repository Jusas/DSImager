namespace DSImager.Core.Interfaces
{
    public interface IViewProvider
    {
        void Register<TView, TViewModel>()
            where TView : IView<TViewModel>
            where TViewModel : IViewModel<TViewModel>;

        IView<TViewModel> Instantiate<TViewModel>() where TViewModel : IViewModel<TViewModel>;
    }
}
