using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Application.Views;
using DSImager.Core.Interfaces;

namespace DSImager.Application
{
    class WpfApplication : IApplication
    {

        private System.Windows.Application _application;
        private MainWindow _mainWindow { get { return (MainWindow)_application.MainWindow; } }
        
        public WpfApplication()
        {
            _application = System.Windows.Application.Current;
        }

        public event EventHandler OnAppStartUp;
        public event EventHandler OnAppExit;
        public event EventHandler<bool> BeforeAppExit;
        public void ExitApplication(int exitCode)
        {
            throw new NotImplementedException();
        }
    }
}
