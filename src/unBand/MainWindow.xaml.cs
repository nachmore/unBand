using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using unBand.pages;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;
using unBand.BandHelpers;
using System.Timers;

namespace unBand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            Telemetry.TrackEvent(TelemetryCategory.General, Telemetry.TelemetryEvent.AppLaunch, Settings.Current.Device);

            string message = null;

            if (!BandManager.CanRun(ref message))
            {
                MessageBox.Show(message, "unBand Startup Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                Application.Current.Shutdown();

                return;
            }

            // Create just creates the singleton - call Start() to actually get things rolling
            BandManager.Create();
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Settings.Current.AgreedToFirstRunWarning)
            {
                if (!await AgreeToFirstRunWarning())
                {
                    Telemetry.TrackEvent(TelemetryCategory.General, Telemetry.TelemetryEvent.DeclinedFirstRunWarning);
                    Application.Current.Shutdown();
                }
            }

            if (!Settings.Current.AgreedToTelemetry)
            {
                if (!await AgreeToTelemetry())
                {
                    Telemetry.TrackEvent(TelemetryCategory.General, Telemetry.TelemetryEvent.DeclinedTelemetry);
                    Application.Current.Shutdown();
                }
            }

            // Begin Band detection - this will continue for the lifetime of the process
            BandManager.Start();

            if (About.Current.WasUpdated)
            {
                Telemetry.TrackEvent(TelemetryCategory.General, Telemetry.TelemetryEvent.AppUpgraded);
                PageContent.Content = new AboutPage(true);
            }
            else
            {
                ButtonMyBand_Click(null, null);
            }
        }

        private async Task<bool> AgreeToFirstRunWarning()
        {
            var dialogSettings = new MetroDialogSettings();
            dialogSettings.AffirmativeButtonText = "Understood";
            dialogSettings.NegativeButtonText = "Exit";

            var dialogResult = await this.ShowMessageAsync("Hello", 
                "WARNING: This software is not supported by Microsoft, we take no responsibility for what may happen to your Band.\n\n... and no, we haven't seen anything bad... yet.", 
                MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                Settings.Current.AgreedToFirstRunWarning = true;
                return true;
            }

            return false;
        }

        private async Task<bool> AgreeToTelemetry()
        {
            var dialogSettings = new MetroDialogSettings(); 
            dialogSettings.AffirmativeButtonText = "I agree";
            dialogSettings.NegativeButtonText = "Exit";

            var dialogResult = await this.ShowMessageAsync("Heads Up",
                "We track usage of certain portions of this application, such as section usage and failure information. We don't collect anything personal (ever), but you should still know about it.\n\n", 
                MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                Settings.Current.AgreedToTelemetry = true;
                return true;
            }

            return false;
        }

        private void ButtonMyBand_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new MyBandPage();
        }

        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new ThemePage();
        }

        private void ButtonSensors_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new SensorsPage();
        }

        public void Navigate(UserControl content)
        {
            PageContent.Content = content;
        }

        private void MetroWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Settings.Current.Save();
        }

        private void ButtonLog_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new LogOutputPage();
        }

        private void ButtonTiles_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new TilesPage();
        }

        private void ButtonActivityLog_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new ActivityLogPage();
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new AboutPage();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new SettingsPage();
        }
        
    }
}
