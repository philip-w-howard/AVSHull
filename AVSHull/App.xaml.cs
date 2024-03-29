﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Referenced in All.xaml (commented out)
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            FatalError errorWindow = new FatalError();
            errorWindow.Owner = MainWindow;
            errorWindow.ShowDialog();
            Shutdown();
        }
    }
}
