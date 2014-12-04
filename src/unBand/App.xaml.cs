using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace unBand
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            unBand.Properties.Settings.Default.Save();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Telemetry.Client.TrackException(e.Exception);

            MessageBox.Show("An unhandled exception occurred - sorry about that, we're going to have to crash now :(\n\nYou can open a bug with a copy of this crash: hit Ctrl + C right now and then paste into a new bug at https://github.com/nachmore/unBand/issues.\n\n" + e.Exception.ToString(),
                "Imminent Crash", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
