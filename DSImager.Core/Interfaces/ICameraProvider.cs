using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;

namespace DSImager.Core.Interfaces
{
    public interface ICameraProvider
    {
        string ChooseCameraDeviceId();
        ICameraV2 GetCamera(string deviceId);
    }
}
