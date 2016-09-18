using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DSImager.Application.Views;
using DSImager.Core.Interfaces;
using MahApps.Metro;

namespace DSImager.Application
{
    /// <summary>
    /// Implementation of IApplication.
    /// Provides common application functionality.
    /// </summary>
    class WpfApplication : IApplication
    {

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES AND EVENTS
        //-------------------------------------------------------------------------------------------------------

        private System.Windows.Application _application { get { return System.Windows.Application.Current; } }
        private MainWindow _mainWindow { get { return (MainWindow)_application.MainWindow; } }

        public event ApplicationLightOverlayModeHandler OnLightOverlayModeSet;
        public event ApplicationSessionVariableChangedHandler OnSessionVariableChanged;
        public event EventHandler OnAppStartUp;
        public event EventHandler OnAppExit;
    
        private const string WhiteThemeName = "WhiteTheme";
        private const string WhiteAccentName = "WhiteAccent";

        private Dictionary<string, object> _sessionVariables = new Dictionary<string, object>();

        private IAppVisualThemeManager _appVisualThemeManager;

        public bool IsInLightOverlayMode
        {
            get { return _appVisualThemeManager.GetCurrentTheme() == WhiteThemeName; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region METHODS
        //-------------------------------------------------------------------------------------------------------

        public WpfApplication(IAppVisualThemeManager appVisualThemeManager)
        {
            _appVisualThemeManager = appVisualThemeManager;
        }

        public void Initialize()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            
            _application.Exit += ApplicationOnExit;
            _application.Startup += ApplicationOnStartup;   
        }

        private void ApplicationOnStartup(object sender, StartupEventArgs startupEventArgs)
        {
            if (OnAppStartUp != null)
                OnAppStartUp(this, startupEventArgs);
        }

        private void ApplicationOnExit(object sender, ExitEventArgs exitEventArgs)
        {
            if (OnAppExit != null)
                OnAppExit(this, exitEventArgs);
        }


        public void ExitApplication(int exitCode)
        {
            _application.Shutdown(exitCode);
        }

        public void SetLightOverlayMode(bool lightOverlayMode)
        {
            if (lightOverlayMode)
            {
                _appVisualThemeManager.SetTheme(WhiteThemeName, WhiteAccentName);
            }
            else
            {
                _appVisualThemeManager.SetTheme(_appVisualThemeManager.StandardTheme, _appVisualThemeManager.StandardAccent);
            }
            OnLightOverlayModeSet?.Invoke(lightOverlayMode);
        }

        public void SetApplicationSessionVariable(string name, object variable)
        {
            object oldval = null;
            if (!_sessionVariables.ContainsKey(name))
                _sessionVariables.Add(name, variable);
            else
            {
                oldval = _sessionVariables[name];
                _sessionVariables[name] = variable;
            }
                
            OnSessionVariableChanged?.Invoke(name, oldval, variable);
        }

        public object GetApplicationSessionVariable(string name)
        {
            if (_sessionVariables.ContainsKey(name))
                return _sessionVariables[name];
            return null;
        }

        #endregion

    }
}
