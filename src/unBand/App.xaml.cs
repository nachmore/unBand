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
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Telemetry.TrackException(e.Exception);

            if (e.Exception is ArgumentException)
            {
                if (e.Exception.Message.Contains("name cannot have length 0"))
                {
                    MessageBox.Show("Your Band is hitting a Tile bug that unfortunately unBand cannot work around.\n\nTo fix this please remove all of your tiles through the Phone app and the readd them back, this should fix this error. Sadly, unBand will now close.\n\nIf it doesn't, please open a bug at https://github.com/nachmore/unBand/issues with the contents of this message (hit Ctrl+C now to copy)\n\n" + e.Exception.ToString());
                    return;
                }
            }

            MessageBox.Show("An unhandled exception occurred - sorry about that, we're going to have to crash now :(\n\nYou can open a bug with a copy of this crash: hit Ctrl + C right now and then paste into a new bug at https://github.com/nachmore/unBand/issues.\n\n" + e.Exception.ToString(),
                "Imminent Crash", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
