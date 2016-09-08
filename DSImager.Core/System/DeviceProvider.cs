using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;

namespace DSImager.Core.System
{
    [Obsolete("Replaced with specialized device providers")]
    public class DeviceProvider : IDeviceProvider
    {
        private SimpleInjector.Container _container;
        private Dictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();

        public DeviceProvider(SimpleInjector.Container container)
        {
            _container = container;
        }

        public void Register<TIDevice, TDevice>() 
        {
            var interfaceType = typeof (TIDevice);
            var implType = typeof (TDevice);
            _typeMap.Add(interfaceType, implType);            
        }

        public TIDevice Instantiate<TIDevice>()
        {
            var interfaceType = typeof(TIDevice);
            var implType = _typeMap[interfaceType];
            var instance = _container.GetInstance(implType);
            return (TIDevice)instance;
        }
    }
}
