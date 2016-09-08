using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using ASCOM.DriverAccess;
using DSImager.Core.Interfaces;
using DSImager.Core.Services;
using DSImager.Core.System;
using DSImager.ViewModels;
using SimpleInjector;

namespace DSImager.Tests
{
    /// <summary>
    /// Bootstrapper class, for registering classes to SimpleInjector.
    /// Also registers View-ViewModel pairs to ViewProvider.
    /// </summary>
    public static class Bootstrapper
    {

        public static Container Container { get; set; }

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

        public static Container DefaultBootstrap()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var container = new Container();

            container.Register<IViewProvider, ViewProvider>(Lifestyle.Singleton);
            container.Register<ILogService, LogService>(Lifestyle.Singleton);
            container.Register<IStorageService, StorageService>(Lifestyle.Singleton);
            container.Register<ICameraService, CameraService>(Lifestyle.Singleton);
            container.Register<IImagingService, ImagingService>(Lifestyle.Singleton);
            container.Register<IImageIoService, ImageIoService>(Lifestyle.Singleton);

            var viewModelTypes =
                GetAllTypesImplementingOpenGenericType(typeof(IViewModel<>), assembly).ToList();
            viewModelTypes.ForEach(t => container.Register(t));

            var viewTypes =
                GetAllTypesImplementingOpenGenericType(typeof(IView<>), assembly).ToList();
            viewTypes.ForEach(t => container.Register(t));

            var viewProvider = container.GetInstance<IViewProvider>();
            
            var imageIoService = container.GetInstance<IImageIoService>();
            var fitsWriter = new FitsWriter();
            imageIoService.RegisterImageWriter(fitsWriter.Format, fitsWriter);

            container.Verify();
            Container = container;
            return container;
        }
    }
}
