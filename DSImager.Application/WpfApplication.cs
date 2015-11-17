using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Application.Views;
using DSImager.Core.Interfaces;

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

        public event EventHandler OnAppStartUp;
        public event EventHandler OnAppExit;

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region METHODS
        //-------------------------------------------------------------------------------------------------------

        public WpfApplication()
        {
        }
        

        public void ExitApplication(int exitCode)
        {
            _application.Shutdown(exitCode);
        }

        #endregion

    }
}
