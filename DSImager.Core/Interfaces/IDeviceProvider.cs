using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public interface IDeviceProvider
    {
        void Register<TIDevice, TDevice>();
        TIDevice Instantiate<TIDevice>();
    }
}
