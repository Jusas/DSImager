using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ASCOM.DeviceInterface;
using ASCOM.DriverAccess;
using DSImager.Application.AppPlatform;
using DSImager.Application.Utils;
using DSImager.Application.Views;
using DSImager.Core.Devices;
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

            container.Register<ISystemEnvironment, SystemEnvironment>(Lifestyle.Singleton);
            container.Register<IAppVisualThemeManager, AppVisualThemeManager>(Lifestyle.Singleton);
            container.Register<IProgramSettingsManager, ProgramSettingsManager>(Lifestyle.Singleton);
            container.Register<IApplication, WpfApplication>(Lifestyle.Singleton);
            container.Register<IViewProvider, ViewProvider>(Lifestyle.Singleton);
            container.Register<IDialogProvider, DialogProvider>();
            container.Register<ICameraProvider, CameraProvider>(Lifestyle.Singleton);
            container.Register<ILogService, LogService>(Lifestyle.Singleton);
            container.Register<IStorageService, StorageService>(Lifestyle.Singleton);
            container.Register<ICameraService, CameraService>(Lifestyle.Singleton);
            container.Register<IImagingService, ImagingService>(Lifestyle.Singleton);
            container.Register<IImageIoService, ImageIoService>(Lifestyle.Singleton);
            
            
 
            var viewModelTypes =
                GetAllTypesImplementingOpenGenericType(typeof (IViewModel<>), assembly).ToList();
            viewModelTypes.ForEach(t => container.Register(t));

            var viewTypes =
                GetAllTypesImplementingOpenGenericType(typeof (IView<>), assembly).ToList();
            viewTypes.ForEach(t => container.Register(t));

            var viewProvider = container.GetInstance<IViewProvider>();
            viewProvider.Register<ConnectDialog, ConnectDialogViewModel>();
            viewProvider.Register<DeviceInfoDialog, DeviceInfoViewModel>();
            viewProvider.Register<HistogramDialog, HistogramDialogViewModel>();
            viewProvider.Register<SessionDialog, SessionDialogViewModel>();
            viewProvider.Register<TemperatureDialog, TemperatureDialogViewModel>();
            viewProvider.Register<BiasFrameDialog, BiasFrameDialogViewModel>();
            viewProvider.Register<DarkFrameDialog, DarkFrameDialogViewModel>();
            viewProvider.Register<FlatFrameDialog, FlatFrameDialogViewModel>();

            var imageIoService = container.GetInstance<IImageIoService>();
            var fitsWriter = new FitsWriter();
            imageIoService.RegisterImageWriter(fitsWriter.Format, fitsWriter);

            container.Verify();
            return container;
        }
    }
}
