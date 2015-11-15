using System;
using System.Collections.Generic;
using DSImager.Core.Interfaces;
using SimpleInjector;

namespace DSImager.Core.System
{
    public class ViewProvider : IViewProvider
    {
        private SimpleInjector.Container _container;
        private Dictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();

        public ViewProvider(SimpleInjector.Container container)
        {
            _container = container;
        }

        public void Register<TView, TViewModel>() 
            where TView: IView<TViewModel> 
            where TViewModel: IViewModel<TViewModel>
        {
            var vmType = typeof (TViewModel);
            var vType = typeof (TView);
            _typeMap.Add(vmType, vType);            
        }

        public IView<TViewModel> Instantiate<TViewModel>() where TViewModel:IViewModel<TViewModel>
        {
            var vmType = typeof (TViewModel);
            var viewType = _typeMap[vmType];
            var instance = _container.GetInstance(viewType);
            return (IView<TViewModel>) instance;
        }
    }
}
