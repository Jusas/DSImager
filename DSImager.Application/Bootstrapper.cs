using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSImager.Application.Views;
using DSImager.Core.Interfaces;
using DSImager.Core.Services;
using DSImager.Core.System;
using DSImager.ViewModels;
using SimpleInjector;

namespace DSImager.Application
{
    /// <summary>
    /// Bootstrapper class, for registering classes to SimpleInjector.
    /// Also registers View-ViewModel pairs to ViewProvider.
    /// </summary>
    public static class Bootstrapper
    {
        private static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType, Assembly assembly)
        {
            var types = assembly.GetTypes();
            var validTypes = new List<Type>();
            foreach (var t in types)
            {
                var baseType = t.BaseType;
                var interfaces = t.GetInterfaces();
                bool isMatch =
                    interfaces.Any(i => i.IsGenericType && openGenericType.IsAssignableFrom(i.GetGenericTypeDefinition()));
                if (!isMatch)
                    isMatch = baseType != null && baseType.IsGenericType &&
                              openGenericType.IsAssignableFrom(baseType.GetGenericTypeDefinition());
                if (isMatch && !t.IsAbstract)
                    validTypes.Add(t);

            }

            return validTypes;
        }

        public static Container Bootstrap()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var container = new Container();

            container.Register<IApplication, WpfApplication>(Lifestyle.Singleton);
            container.Register<IViewProvider, ViewProvider>(Lifestyle.Singleton);
            container.Register<ILogService, LogService>(Lifestyle.Singleton);
            container.Register<ICameraService, CameraService>(Lifestyle.Singleton);

            var viewModelTypes =
                GetAllTypesImplementingOpenGenericType(typeof (IViewModel<>), assembly).ToList();
            viewModelTypes.ForEach(t => container.Register(t));

            var viewTypes =
                GetAllTypesImplementingOpenGenericType(typeof (IView<>), assembly).ToList();
            viewTypes.ForEach(t => container.Register(t));

            var viewProvider = container.GetInstance<IViewProvider>();
            viewProvider.Register<ConnectDialog, ConnectDialogViewModel>();
            viewProvider.Register<DeviceInfoDialog, DeviceInfoViewModel>();


            container.Verify();
            return container;
        }
    }
}
