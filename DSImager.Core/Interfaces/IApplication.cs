using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public interface IApplication
    {
        event EventHandler OnAppStartUp;
        event EventHandler OnAppExit;

        void ExitApplication(int exitCode);

    }
}
