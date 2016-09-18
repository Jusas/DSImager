using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public delegate void ApplicationLightOverlayModeHandler(bool lightOverlayMode);
    public delegate void ApplicationSessionVariableChangedHandler(string name, object oldVariable, object newVariable);

    public interface IApplication
    {
        bool IsInLightOverlayMode { get; }
        event ApplicationLightOverlayModeHandler OnLightOverlayModeSet;
        event ApplicationSessionVariableChangedHandler OnSessionVariableChanged;

        event EventHandler OnAppStartUp;
        event EventHandler OnAppExit;

        void ExitApplication(int exitCode);

        void SetLightOverlayMode(bool lightOverlayMode);
        void SetApplicationSessionVariable(string name, object variable);
        object GetApplicationSessionVariable(string name);
    }
}
