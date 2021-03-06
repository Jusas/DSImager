﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSImager.Application;
using DSImager.Application.Views;
using DSImager.Core.Interfaces;
using DSImager.ViewModels;
using MahApps.Metro;
using SimpleInjector;

namespace DSImager
{
    static class Program
    {
        
        [STAThread]
        static void Main()
        {
            RunApp(Bootstrapper.Bootstrap());
        }

        private static void RunApp(Container container)
        {
            try
            {
                XamlGeneratedNamespace.GeneratedApplication app = new XamlGeneratedNamespace.GeneratedApplication();
                ((WpfApplication)container.GetInstance<IApplication>()).Initialize();
                app.InitializeComponent();
                app.Startup += (sender, args) =>
                {
                    ThemeManager.AddAccent("WhiteAccent",
                        new Uri("pack://application:,,,/DSImager.Application;component/Themes/WhiteAccent.xaml"));
                    ThemeManager.AddAppTheme("WhiteTheme",
                        new Uri("pack://application:,,,/DSImager.Application;component/Themes/WhiteTheme.xaml"));
                };
                var mainWin = container.GetInstance<MainWindow>();
                container.GetInstance<ILogService>().Trace(LogEventCategory.Informational, "App is starting");
                app.Run(mainWin);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}
