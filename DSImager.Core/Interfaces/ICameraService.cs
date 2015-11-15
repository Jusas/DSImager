using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public interface ICameraService
    {
        bool Initialized { get; }
        string LastError { get; }

        string ChooseDevice();
        bool Initialize(string deviceId);
    }
}
